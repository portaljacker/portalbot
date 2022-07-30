namespace PortalBot;

using DarkSky.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Jurassic;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Processors;

public class Program
{
    private readonly DiscordSocketConfig _socketConfig;
    private readonly IServiceProvider _services;

    public Program()
    {
        _socketConfig = new() { GatewayIntents = GatewayIntents.AllUnprivileged };
        _services = ConfigureServices();
    }

    public static void Main() => new Program().RunAsync().GetAwaiter().GetResult();

    private async Task RunAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

        var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Block this task until the program is exited.
        await Task.Delay(-1);
    }

    private IServiceProvider ConfigureServices()
    {
        var darkSkySecretKey = Environment.GetEnvironmentVariable("DARK_SKY_SECRET_KEY");

        return new ServiceCollection()
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(s => new InteractionService(s.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton(new HttpClient())
            .AddSingleton(new DarkSkyService(darkSkySecretKey))
            .AddSingleton<WeatherProcessor>()
            .AddSingleton(new Dictionary<string, Fact>())
            .AddSingleton(new ScriptEngine())
            .AddSingleton(new Random())
            .AddSingleton<FactProcessor>()
            .BuildServiceProvider();
    }

    private async Task LogAsync(LogMessage message) => Console.WriteLine(message.ToString());

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
