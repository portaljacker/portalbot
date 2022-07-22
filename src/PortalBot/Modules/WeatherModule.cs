namespace PortalBot.Modules;

using Discord.Commands;
using PortalBot.Enums;
using PortalBot.Models;
using PortalBot.Processors;

[Group("weather")]
[Alias("woppy")]
public class WeatherModule : ModuleBase
{
    private readonly WeatherProcessor _weather;

    public WeatherModule(WeatherProcessor weather) => _weather = weather;

    [Command]
    [Priority(-1)]
    [Summary("Get the weather for the selected city.")]
    public async Task Weather([Remainder, Summary("City (State optional")] string city) => await ReplyAsync(await _weather.GetWeather(new WeatherRequest(city, DarkSkyUnits.Auto)));

    [Command("ca")]
    [Alias("c", "C")]
    [Summary("Get the Canadian weather for the selected city.")]
    public async Task WeatherCa([Remainder, Summary("City (State optional")] string city) => await ReplyAsync(await _weather.GetWeather(new WeatherRequest(city, DarkSkyUnits.Ca)));

    [Command("uk")]
    [Alias("u", "U")]
    [Summary("Get the Queen's weather for the selected city.")]
    public async Task WeatherUk([Remainder, Summary("City (State optional")] string city) => await ReplyAsync(await _weather.GetWeather(new WeatherRequest(city, DarkSkyUnits.Uk2)));

    [Command("us")]
    [Alias("f", "F")]
    [Summary("Get the freedom weather for the selected city.")]
    public async Task WeatherUs([Remainder, Summary("City (State optional")] string city) => await ReplyAsync(await _weather.GetWeather(new WeatherRequest(city, DarkSkyUnits.Us)));

    [Command("si")]
    [Alias("s", "S")]
    [Summary("Get the scientific weather for the selected city.")]
    public async Task WeatherSi([Remainder, Summary("City (State optional")] string city) => await ReplyAsync(await _weather.GetWeather(new WeatherRequest(city, DarkSkyUnits.Si)));
}
