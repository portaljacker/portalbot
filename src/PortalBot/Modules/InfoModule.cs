namespace PortalBot.Modules;

using Discord.Interactions;

public class InfoModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if bot is online")]
    public async Task Ping() => await RespondAsync("pong!");

    [SlashCommand("say", "Echos a message")]
    public async Task Say([Summary(description: "The text to say")] string echo)
    {
        var userInfo = Context.User;
        await RespondAsync($"{userInfo.Username} wants me to say, \"{echo}\" frankly I think that's a ridiculous expectation on their part, now don't you?");
    }
}