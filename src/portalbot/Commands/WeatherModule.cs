using DarkSky.Models;
using DarkSky.Services;
using Discord.Commands;
using Newtonsoft.Json;
using portalbot.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace portalbot.Commands
{
    [Group("weather")]
    [Alias("woppy")]
    public class WeatherModule : ModuleBase
    {
        private static readonly Regex ValidCityState = new Regex(@"^([\p{L}\p{Mn}]+(?:[\s-'][\p{L}\p{Mn}]+)*)(?:(\,|(\,\s))[\p{L}\p{Mn}]{2,})?$", RegexOptions.IgnoreCase);

        private readonly string _googleApiToken = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");

        private readonly HttpClient _httpClient;
        private readonly DarkSkyService _darkSky;

        public WeatherModule(HttpClient httpClient, DarkSkyService darkSky)
        {
            _httpClient = httpClient;
            _darkSky = darkSky;
        }

        [Command]
        [Alias("f", "F")]
        [Summary("Get the weather for the selected city.")]
        public async Task Weather([Remainder, Summary("City (State optional")] string city)
        {
            if (!ValidCityState.IsMatch(city))
            {
                await ReplyAsync("Invalid city format, please try again. (eg. Washington, DC)");
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
            }

            var forecast = await GetWeather(location);

            await SendWeather(cityString, forecast.Response.Currently);
        }

        [Command("c")]
        [Alias("C")]
        [Summary("Get the real weather for the selected city.")]
        public async Task RealWeather([Remainder, Summary("City (State optional")] string city)
        {
            if (!ValidCityState.IsMatch(city))
            {
                await ReplyAsync("Invalid city format, please try again. (eg. Montreal, QC)");
                return;
            }

            var location = await GetLocation(city);
            if (location == null)
            {
                await ReplyAsync("Error Querying Google API.");
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
                        await ReplyAsync("Location entered is not a city (or is not specific enough)");
                        return;
                    }

                    cityString += $", {addressComponent.ShortName}";
                }
            }

            var forecast = await GetWeather(location, true);

            await SendWeather(cityString, forecast.Response.Currently, true);
        }

        private async Task<GeocoderResponse> GetLocation(string address)
        {
            var requestString = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={_googleApiToken}";
            var response = await _httpClient.GetAsync(requestString);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GeocoderResponse>(responseString);
        }

        private async Task<DarkSkyResponse> GetWeather(GeocoderResponse location, bool canada = false)
        {
            if (canada)
            {
                return await _darkSky.GetForecast((double)location.Results[0].Geometry.Location.Lat, (double)location.Results[0].Geometry.Location.Lng,
                    new DarkSkyService.OptionalParameters()
                    {
                        MeasurementUnits = "ca"
                    });
            }
            else
                return await _darkSky.GetForecast((double)location.Results[0].Geometry.Location.Lat, (double)location.Results[0].Geometry.Location.Lng);
        }

        private async Task SendWeather(string city, DataPoint currently, bool canada = false)
        {
            if (currently.Temperature != null && currently.WindSpeed != null)
            {
                if (canada)
                    await ReplyAsync($"Weather in ***{city}*** " +
                                     $"is currently ***{(int)currently.Temperature}° C***, " +
                                     $"with wind speed of ***{(int)currently.WindSpeed} km/h***.");
                else
                    await ReplyAsync($"Weather in ***{city}*** " +
                                     $"is currently ***{(int)currently.Temperature}° F***, " +
                                     $"with wind speed of ***{(int)currently.WindSpeed} mph***.");
            }
        }
    }
}
