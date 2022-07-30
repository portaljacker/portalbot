namespace PortalBot;

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly ulong _testGuidId;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
    {
        _client = client;
        _handler = handler;
        _services = services;
        var parsed = ulong.TryParse(Environment.GetEnvironmentVariable("DISCORD_TEST_GUILD_ID"), out var testGuildId);
        _testGuidId = parsed ? testGuildId : 0;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteraction;
    }

    private async Task LogAsync(LogMessage message) => Console.WriteLine(message.ToString());

    private async Task ReadyAsync()
    {
        if (Program.IsDebug())
        {
            await _handler.RegisterCommandsToGuildAsync(_testGuidId, true);
        }
        else
        {
            await _handler.RegisterCommandsGloballyAsync(true);
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);

            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
            }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
