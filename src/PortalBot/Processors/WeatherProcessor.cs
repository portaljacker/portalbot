namespace PortalBot.Processors;

using System.Text.Json;
using System.Text.RegularExpressions;
using DarkSky.Models;
using DarkSky.Services;
using Enums;
using Models;

public class WeatherProcessor
{
    private static readonly Regex s_validCityState = new(@"^([\p{L}\p{Mn}]+(?:[\s-'][\p{L}\p{Mn}]+)*)(?:(\,|(\,\s))[\p{L}\p{Mn}]{2,})?$", RegexOptions.IgnoreCase);

    private readonly string? _googleApiToken = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    private readonly HttpClient _httpClient;
    private readonly DarkSkyService _darkSky;

    public WeatherProcessor(HttpClient httpClient, DarkSkyService darkSky)
    {
        _httpClient = httpClient;
        _darkSky = darkSky;
    }

    public async Task<string> GetWeather(WeatherRequest request)
    {
        if (!s_validCityState.IsMatch(request.Location))
        {
            var example = request.Units == DarkSkyUnits.Ca ? "Montreal, QC" : "Washington, DC";

            return $"Invalid city format, please try again. (eg. {example})";
        }

        var location = await GetLocation(request.Location);
        if (location == null)
        {
            return "Error Querying Google API.";
        }

        if (location.Results.Length <= 0)
        {
            return "No results found.";
        }

        if (location.Results[0].Geometry.Location.Lat == 0 && location.Results[0].Geometry.Location.Lng == 0)
        {
            return "City not found, please try again.";
        }

        var cityString = "";
        var cityType = location.Results[0].Types;
        var provinceType = new[] { "administrative_area_level_1", "political" };
        var stateType = new[] { "administrative_area_level_1", "establishment", "point_of_interest", "political" };
        var countryType = new[] { "country", "political" };
        if (cityType.SequenceEqual(provinceType) || cityType.SequenceEqual(stateType) || cityType.SequenceEqual(countryType))
        {
            return "Location entered is not a city (or is not specific enough).";
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
                    return "Location entered is not a city.";
                }

                cityString += $", {addressComponent.ShortName}";
            }

            if (cityString != "")
            {
                continue;
            }

            return "Location entered is not a city.";
        }

        var forecast = await GetWeather(location, request.Units);

        return FormatWeather(cityString, forecast);
    }

    private async Task<GeocoderResponse?> GetLocation(string address)
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

    private static string FormatWeather(string city, DarkSkyResponse forecast)
    {
        var currently = forecast.Response.Currently;
        var units = forecast.Response.Flags.Units;

        if (currently.Temperature == null || currently.WindSpeed == null)
        {
            return "";
        }

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

        return $"Weather in ***{city}*** " +
               $"is currently ***{(int)currently.Temperature}° {tempUnit}***, " +
               $"{currently.Summary.ToLowerInvariant()}, " +
               $"with wind speed of ***{(int)currently.WindSpeed} {windSpeedUnit}***.";

    }
}
