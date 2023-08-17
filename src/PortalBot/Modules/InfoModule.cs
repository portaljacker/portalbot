namespace PortalBot.Modules;

using Discord;
using Discord.Interactions;

public class InfoModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if bot is online")]
    public async Task Ping() => await RespondAsync("pong!");

    [SlashCommand("say", "Echos a message")]
    public async Task Say([Summary(description: "The text to say")] string echo)
    {
        var userInfo = Context.User;
        await RespondAsync($"{Format.Sanitize(userInfo.Username)} wants me to say, \"{Format.Sanitize(echo)}\" frankly I think that's a ridiculous expectation on their part, now don't you?");
    }
}
