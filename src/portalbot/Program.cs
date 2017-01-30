﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace portalbot
{
    class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private DependencyMap _map;

        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();

        public async Task Run()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            string token = "aaabbbcc";

            _map = new DependencyMap();

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

            int argPos = 0;

            if (!message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new CommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _map);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}