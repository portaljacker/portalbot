namespace PortalBot.Modules;

using Discord.Commands;
using PortalBot.Processors;

[Group("snapple")]
public class SnappleModule : ModuleBase
{
    private FactProcessor _facts;

    public SnappleModule(FactProcessor facts)
    {
        _facts = facts;
    }

    [Command]
    [Summary("Get a random Snapple \"Real Fact\"")]
    public async Task GetFact() => await ReplyAsync("", embed: _facts.GetFact());
}
