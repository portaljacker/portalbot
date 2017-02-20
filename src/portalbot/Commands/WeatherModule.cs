using System;
using DarkSky.Services;
using Discord.Commands;
using GoogleGeoCoderCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DarkSky.Models;

namespace portalbot.Commands
{
    [Group("weather")]
    [Alias("woppy")]
    public class WeatherModule : ModuleBase
    {
        private static readonly Regex ValidCityState = new Regex(@"^[\p{L}\p{Mn}]+(?:[\s-][\p{L}\p{Mn}]+)*(?:(\,|(\,\s))?[\p{L}\p{Mn}]{2,})$", RegexOptions.IgnoreCase);

        private readonly GoogleGeocodeService _geocoder;
        private readonly DarkSkyService _darkSky;

        public WeatherModule(GoogleGeocodeService geocoder, DarkSkyService darkSky)
        {
            _geocoder = geocoder;
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

            const double tolerance = 0.001f;
            if (Math.Abs(location.Latitude) < tolerance && Math.Abs(location.Longitude) < tolerance)
            {
                await ReplyAsync("City not found, please try again.");
                return;
            }

            var forecast = await GetWeather(location);

            await SendWeather(city, forecast.Response.Currently);
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

            const double tolerance = 0.001f;
            if (Math.Abs(location.Latitude) < tolerance && Math.Abs(location.Longitude) < tolerance)
            {
                await ReplyAsync("City not found, please try again.");
                return;
            }

            var forecast = await GetWeather(location, true);

            await SendWeather(city, forecast.Response.Currently, true);
        }

        private async Task<Location> GetLocation(string address)
        {
            return await _geocoder.GeocodeLocation(address);
        }

        private async Task<DarkSkyResponse> GetWeather(Location location, bool canada = false)
        {
            if (canada)
            {
                return await _darkSky.GetForecast(location.Latitude, location.Longitude,
                    new DarkSkyService.OptionalParameters()
                    {
                        MeasurementUnits = "ca"
                    });
            }
            else
                return await _darkSky.GetForecast(location.Latitude, location.Longitude);
        }

        private async Task SendWeather(string city, DataPoint currently, bool canada = false)
        {
            if (currently.Temperature != null && currently.WindSpeed != null)
            {
                if (canada)
                    await ReplyAsync($"Weather in ***{city}*** is currently ***{(int)currently.Temperature}° C***, with wind speed of ***{(int)currently.WindSpeed} km/h***.");
                else
                    await ReplyAsync($"Weather in ***{city}*** is currently ***{(int)currently.Temperature}° F***, with wind speed of ***{(int)currently.WindSpeed} mph***.");
            }
        }
    }
}
