namespace PortalBot.Modules;

using Discord.Interactions;
using Enums;
using Models;
using Processors;

public class WeatherModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WeatherProcessor _weather;

    public WeatherModule(WeatherProcessor weather) => _weather = weather;

    [SlashCommand("weather", "Get the weather for the selected city")]
    public async Task Weather(
        [Summary(description: "City (State optional)")] string city,
        [Summary(description: "Regional unit format")] DarkSkyUnits units = DarkSkyUnits.Auto)
        => await RespondAsync(await _weather.GetWeather(new WeatherRequest(city, units)));

    [SlashCommand("woppy", "Ask Woppy the weather robot about the weather for the selected city")]
    public async Task Woppy(
        [Summary(description: "City (State optional)")] string city,
        [Summary(description: "Regional unit format")] DarkSkyUnits units = DarkSkyUnits.Auto)
        => await Weather(city, units);
}
