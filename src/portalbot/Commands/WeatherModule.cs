using System;
using DarkSky.Services;
using Discord.Commands;
using GoogleGeoCoderCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace portalbot.Commands
{
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

        [Command("weather")]
        [Summary("Echos a message.")]
        [Alias("woppy")]
        public async Task Weather([Remainder, Summary("City (State optional")] string city)
        {
            if (!ValidCityState.IsMatch(city))
            {
                await ReplyAsync("Invalid city format, please try again. (eg. Montreal, QC)");
                return;
            }

            var location = GetLocation(city);

            const double tolerance = 0.001f;
            if (Math.Abs(location.Result.Latitude) < tolerance && Math.Abs(location.Result.Longitude) < tolerance)
            {
                await ReplyAsync("City not found, please try again.");
                return;
            }

            var forecast = _darkSky.GetForecast(location.Result.Latitude, location.Result.Longitude);
            var currently = forecast.Result.Response.Currently;

            if (currently.Temperature != null && currently.WindSpeed != null)
                await ReplyAsync($"Weather in ***{city}*** is currently ***{(int)currently.Temperature}°F***, with wind speed of ***{(int)currently.WindSpeed}mph***.");
        }

        private async Task<Location> GetLocation(string address)
        {
            var result = await _geocoder.GeocodeLocation(address);
            return result;
        }
    }
}
