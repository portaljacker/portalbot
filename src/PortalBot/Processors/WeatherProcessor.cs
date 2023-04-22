namespace PortalBot.Processors;

using System.Text.Json;
using System.Text.RegularExpressions;
using Discord;
using Enums;
using Models;
using OpenWeatherMap.Cache;
using OpenWeatherMap.Cache.Models;

public class WeatherProcessor
{
    private static readonly Regex s_validCityState = new(@"^([\p{L}\p{Mn}]+(?:[\s-'][\p{L}\p{Mn}]+)*)(?:(\,|(\,\s))[\p{L}\p{Mn}]{2,})?$", RegexOptions.IgnoreCase);

    private readonly string? _googleApiToken = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

    private readonly HttpClient _httpClient;
    private readonly IOpenWeatherMapCache _weatherMapCache;

    public WeatherProcessor(HttpClient httpClient, IOpenWeatherMapCache weatherMapCache)
    {
        _httpClient = httpClient;
        _weatherMapCache = weatherMapCache;
    }

    public async Task<Embed> GetWeather(WeatherRequest request)
    {
        if (!s_validCityState.IsMatch(request.Location))
        {
            var example = request.Units == WeatherUnits.Ca ? "Montreal, QC" : "Washington, DC";

            return GetErrorEmbed($"Invalid city format, please try again. (eg. {example})");
        }

        var location = await GetLocation(request.Location);
        if (location == null)
        {
            return GetErrorEmbed("Error Querying Google API.");
        }

        if (location.Results.Length <= 0)
        {
            return GetErrorEmbed("No results found.");
        }

        if (location.Results[0].Geometry.Location is { Lat: 0, Lng: 0 })
        {
            return GetErrorEmbed("City not found, please try again.");
        }

        var cityString = "";
        var cityType = location.Results[0].Types;
        var provinceType = new[] { "administrative_area_level_1", "political" };
        var stateType = new[] { "administrative_area_level_1", "establishment", "point_of_interest", "political" };
        var countryType = new[] { "country", "political" };
        if (cityType.SequenceEqual(provinceType) || cityType.SequenceEqual(stateType) || cityType.SequenceEqual(countryType))
        {
            return GetErrorEmbed("Location entered is not a city (or is not specific enough).");
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
                    return GetErrorEmbed("Location entered is not a city.");
                }

                cityString += $", {addressComponent.ShortName}";
            }

            if (cityString == "")
            {
                return GetErrorEmbed("Location entered is not a city.");
            }
        }

        var readings = await GetWeather(location);

        if (readings.IsSuccessful)
        {
            return FormatWeather(cityString, readings, request.Units);
        }
        else
        {
            var apiErrorCode = readings.Exception?.ApiErrorCode;
            var apiErrorMessage = readings.Exception?.ApiErrorMessage;

            return GetErrorEmbed($"Error retrieving weather readings: {apiErrorMessage}\nCode: {apiErrorCode}");
        }
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

    private async Task<Readings> GetWeather(GeocoderResponse location)
    {
        var lat = (double)location.Results[0].Geometry.Location.Lat;
        var lng = (double)location.Results[0].Geometry.Location.Lng;

        var locationQuery = new Location(lat, lng);

        return await _weatherMapCache.GetReadingsAsync(locationQuery);
    }

    private static Embed FormatWeather(string city, Readings readings, WeatherUnits units)
    {
        double temp;
        string tempUnit;
        double windSpeed;
        string windSpeedUnit;

        switch (units)
        {
            case WeatherUnits.Ca:
                temp = readings.Temperature.DegreesCelsius;
                tempUnit = "C";
                windSpeed = readings.WindSpeed.KilometersPerHour;
                windSpeedUnit = "km/h";
                break;
            case WeatherUnits.Metric:
                temp = readings.Temperature.DegreesCelsius;
                tempUnit = "C";
                windSpeed = readings.WindSpeed.MetersPerSecond;
                windSpeedUnit = "m/s";
                break;
            case WeatherUnits.Uk:
                temp = readings.Temperature.DegreesCelsius;
                tempUnit = "C";
                windSpeed = readings.WindSpeed.MilesPerHour;
                windSpeedUnit = "mph";
                break;
            case WeatherUnits.Us:
            default:
                temp = readings.Temperature.DegreesFahrenheit;
                tempUnit = "F";
                windSpeed = readings.WindSpeed.MilesPerHour;
                windSpeedUnit = "mph";
                break;
        }

        var description = $"Weather in ***{readings.CityName}*** " +
                   $"is currently ***{(int)temp}° {tempUnit}***, " +
                   $"{readings.Weather.FirstOrDefault()?.Description}, " +
                   $"with wind speed of ***{(int)windSpeed} {windSpeedUnit}***.";

        var builder = new EmbedBuilder()
            .WithTitle(city)
            .WithDescription(description)
            .WithUrl($"https://openweathermap.org/city/{readings.CityId}")
            .WithColor(new(0xE36D46))
            .WithFooter(footer => footer
                .WithText("Weather data provided by OpenWeather")
                .WithIconUrl("https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/icons/logo_60x60.png"));

        return builder.Build();
    }

    private static Embed GetErrorEmbed(string error)
    {
        var builder = new EmbedBuilder()
            .WithTitle("Weather Error")
            .WithDescription(error)
            .WithColor(new(0xE36D46));

        return builder.Build();
    }
}
