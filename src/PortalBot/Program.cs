using DarkSky.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace PortalBot
{
    public class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        private async Task Run()
        {
            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = ConfigureServices();

            await InstallCommands();

            /*_client.MessageReceived += async (message) =>
            {
                if (message.Content == "!ping")
                    await message.Channel.SendMessageAsync("pong");
            };//*/

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is exited.
            await Task.Delay(-1);
        }

        private async Task InstallCommands()
        {
            _client.MessageReceived += HandleCommand;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommand(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message)) return;

            var argPos = 0;

            if (!message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new CommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                // Don't report when command doesn't exist.
                if (result is SearchResult)
                    return;
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
                .BuildServiceProvider();
        }
    }
}
