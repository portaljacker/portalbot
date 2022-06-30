namespace PortalBot;

using DarkSky.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jurassic;
using Microsoft.Extensions.DependencyInjection;
using PortalBot.Models;
using System.Reflection;

public class Program : IDisposable
{
    private CommandService _commands = new();
    private DiscordSocketClient _client = new();
    private IServiceProvider? _services;

    public static void Main() => new Program().Run().GetAwaiter().GetResult();

    private async Task Run()
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        _client = new();
        _commands = new();
        _services = ConfigureServices();

        await InstallCommands();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is exited.
        await Task.Delay(-1);
    }

    private async Task InstallCommands()
    {
        _client.MessageReceived += HandleCommand;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommand(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message)
        {
            return;
        }

        var argPos = 0;

        if (!message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
        {
            return;
        }

        CommandContext context = new(_client, message);

        var result = await _commands.ExecuteAsync(context, argPos, _services);
        if (!result.IsSuccess)
        {
            // Don't report when command doesn't exist.
            if (result is SearchResult)
            {
                return;
            }
            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

    private IServiceProvider ConfigureServices()
    {
        var darkSkySecretKey = Environment.GetEnvironmentVariable("DARK_SKY_SECRET_KEY");

        return new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(new HttpClient())
            .AddSingleton(new DarkSkyService(darkSkySecretKey))
            .AddSingleton(new Random())
            .AddSingleton(new Dictionary<string, Fact>())
            .AddSingleton(new ScriptEngine())
            .BuildServiceProvider();
    }

    public void Dispose()
    {
        // uncomment after updating Discord.net
        //this.commands?.Dispose(true);
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
