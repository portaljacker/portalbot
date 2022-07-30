namespace PortalBot.Modules;

using Discord.Interactions;
using Processors;

public class SnappleModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly FactProcessor _facts;

    public SnappleModule(FactProcessor facts)
    {
        _facts = facts;
    }

    [SlashCommand("snapple", "Get a random Snapple \"Real Fact\"")]
    public async Task GetFact() => await RespondAsync("", embed: _facts.GetFact());
}
