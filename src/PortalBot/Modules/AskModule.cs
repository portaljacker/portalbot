namespace PortalBot.Modules;

using System.Collections.ObjectModel;
using Discord.Interactions;

public class AskModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Random _random;
    private readonly IList<string> _answers = new ReadOnlyCollection<string>(new List<string> {
        "It is certain.",
        "It is decidedly so.",
        "Without a doubt.",
        "Yes, definitely.",
        "You may rely on it.",
        "As I see it, yes.",
        "Most likely.",
        "Outlook good.",
        "Yes.",
        "Signs point to yes.",
        "Reply hazy try again.",
        "Ask again later.",
        "Better not tell you now.",
        "Cannot predict now.",
        "Concentrate and ask again.",
        "Don't count on it.",
        "My reply is no.",
        "My sources say no.",
        "Outlook not so good.",
        "Very doubtful."
    });

    public AskModule(Random random) => _random = random;

    [SlashCommand("ask", "Ask a question")]
    public async Task Ask([Summary(description: "Your question")] string question)
    {
        var answer = _answers[_random.Next(_answers.Count)];
        var userInfo = Context.User;
        await RespondAsync($"{userInfo.Username} asked, \"{question}\" Magic 8-ball says: _**\"{answer}\"**_");
    }
}
