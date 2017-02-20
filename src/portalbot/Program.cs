using DarkSky.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Portalbot
{
    public class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private DependencyMap _map;
        private HttpClient _httpClient;
        private DarkSkyService _darkSky;

        public static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
            var darkSkySecretKey = Environment.GetEnvironmentVariable("DARK_SKY_SECRET_KEY");

            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _httpClient = new HttpClient();
            _darkSky = new DarkSkyService(darkSkySecretKey);

            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(_commands);
            _map.Add(_httpClient);
            _map.Add(_darkSky);
            _map.Add(new Random());

            await InstallCommands();

            /*_client.MessageReceived += async (message) =>
            {
                if (message.Content == "!ping")
                    await message.Channel.SendMessageAsync("pong");
            };//*/

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.ConnectAsync();

            // Block this task until the program is exited.
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            _client.MessageReceived += HandleCommand;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            var argPos = 0;

            if (!message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new CommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _map);
            if (!result.IsSuccess)
            {
                // Don't report when command doesn't exist.
                if (result is SearchResult)
                    return;
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
