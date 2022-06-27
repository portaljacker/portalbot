namespace PortalBot.Commands;

using DarkSky.Models;
using DarkSky.Services;
using Discord.Commands;
using PortalBot.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

[Group("weather")]
[Alias("woppy")]
public class WeatherModule : ModuleBase
{
    private static readonly Regex s_validCityState = new(@"^([\p{L}\p{Mn}]+(?:[\s-'][\p{L}\p{Mn}]+)*)(?:(\,|(\,\s))[\p{L}\p{Mn}]{2,})?$", RegexOptions.IgnoreCase);

    private readonly string _googleApiToken = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    private readonly HttpClient _httpClient;
    private readonly DarkSkyService _darkSky;

    public WeatherModule(HttpClient httpClient, DarkSkyService darkSky)
    {
        _httpClient = httpClient;
        _darkSky = darkSky;
    }

    [Command]
    [Priority(-1)]
    [Summary("Get the weather for the selected city.")]
    public async Task Weather([Remainder, Summary("City (State optional")] string city) => await Weather(city, DarkSkyUnits.Auto);

    [Command("ca")]
    [Alias("c", "C")]
    [Summary("Get the Canadian weather for the selected city.")]
    public async Task WeatherCa([Remainder, Summary("City (State optional")] string city) => await Weather(city, DarkSkyUnits.Ca);

    [Command("uk")]
    [Alias("u", "U")]
    [Summary("Get the Queen's weather for the selected city.")]
    public async Task WeatherUk([Remainder, Summary("City (State optional")] string city) => await Weather(city, DarkSkyUnits.Uk2);

    [Command("us")]
    [Alias("f", "F")]
    [Summary("Get the freedom weather for the selected city.")]
    public async Task WeatherUs([Remainder, Summary("City (State optional")] string city) => await Weather(city, DarkSkyUnits.Us);

    [Command("si")]
    [Alias("s", "S")]
    [Summary("Get the scientific weather for the selected city.")]
    public async Task WeatherSi([Remainder, Summary("City (State optional")] string city) => await Weather(city, DarkSkyUnits.Si);

    private async Task Weather(string city, DarkSkyUnits units)
    {
        if (!s_validCityState.IsMatch(city))
        {
            var example = units == DarkSkyUnits.Ca ? "Montreal, QC" : "Washington, DC";

            await ReplyAsync($"Invalid city format, please try again. (eg. {example})");
            return;
        }

        var location = await GetLocation(city);
        if (location == null)
        {
            await ReplyAsync("Error Querying Google API.");
            return;
        }

        if (location.Results.Length <= 0)
        {
            await ReplyAsync("No results found.");
            return;
        }

        if (location.Results[0].Geometry.Location.Lat == 0 && location.Results[0].Geometry.Location.Lng == 0)
        {
            await ReplyAsync("City not found, please try again.");
            return;
        }

        var cityString = "";
        var cityType = location.Results[0].Types;
        var provinceType = new[] { "administrative_area_level_1", "political" };
        var stateType = new[] { "administrative_area_level_1", "establishment", "point_of_interest", "political" };
        var countryType = new[] { "country", "political" };
        if (cityType.SequenceEqual(provinceType) || cityType.SequenceEqual(stateType) || cityType.SequenceEqual(countryType))
        {
            await ReplyAsync("Location entered is not a city (or is not specific enough).");
            return;
        }
        foreach (var addressComponent in location.Results[0].AddressComponents)
        {
            if (addressComponent.Types.SequenceEqual(cityType))
            {
                cityString += addressComponent.LongName;
            }
            else if (addressComponent.Types.SequenceEqual(provinceType))
            {
                if (cityString == "")
                {
                    await ReplyAsync("Location entered is not a city.");
                    return;
                }

                cityString += $", {addressComponent.ShortName}";
            }

            if (cityString != "")
            {
                continue;
            }

            await ReplyAsync("Location entered is not a city.");
            return;
        }

        var forecast = await GetWeather(location, units);

        await SendWeather(cityString, forecast);
    }

    private async Task<GeocoderResponse> GetLocation(string address)
    {
        var requestString = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={_googleApiToken}";
        var response = await _httpClient.GetAsync(requestString);
        if (!response.IsSuccessStatusCode)
        {
            return null; 
        }

        var responseString = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
        };
        return JsonSerializer.Deserialize<GeocoderResponse>(responseString, options);
    }

    private async Task<DarkSkyResponse> GetWeather(GeocoderResponse location, DarkSkyUnits units)
    {
        var lat = (double)location.Results[0].Geometry.Location.Lat;
        var lng = (double)location.Results[0].Geometry.Location.Lng;
        return units switch
        {
            DarkSkyUnits.Auto => await _darkSky.GetForecast(lat, lng, new() { MeasurementUnits = "auto" }),
            DarkSkyUnits.Ca => await _darkSky.GetForecast(lat, lng, new() { MeasurementUnits = "ca" }),
            DarkSkyUnits.Uk2 => await _darkSky.GetForecast(lat, lng, new() { MeasurementUnits = "uk2" }),
            DarkSkyUnits.Us => await _darkSky.GetForecast(lat, lng, new() { MeasurementUnits = "us" }),
            DarkSkyUnits.Si => await _darkSky.GetForecast(lat, lng, new() { MeasurementUnits = "si" }),
            _ => await _darkSky.GetForecast(lat, lng)
        };
    }

    private async Task SendWeather(string city, DarkSkyResponse forecast)
    {
        var currently = forecast.Response.Currently;
        var units = forecast.Response.Flags.Units;

        if (currently.Temperature != null && currently.WindSpeed != null)
        {
            string tempUnit;
            string windSpeedUnit;

            switch (units)
            {
                case "ca":
                    tempUnit = "C";
                    windSpeedUnit = "km/h";
                    break;
                case "uk2":
                    tempUnit = "C";
                    windSpeedUnit = "mph";
                    break;
                case "us":
                    tempUnit = "F";
                    windSpeedUnit = "mph";
                    break;
                default:
                    tempUnit = "C";
                    windSpeedUnit = "m/s";
                    break;
            }

            await ReplyAsync($"Weather in ***{city}*** " +
                             $"is currently ***{(int)currently.Temperature}° {tempUnit}***, " +
                             $"{currently.Summary.ToLower(System.Globalization.CultureInfo.InvariantCulture)}, " +
                             $"with wind speed of ***{(int)currently.WindSpeed} {windSpeedUnit}***.");
        }
    }

    private enum DarkSkyUnits
    {
        Auto,
        Ca,
        Uk2,
        Us,
        Si
    };
}
