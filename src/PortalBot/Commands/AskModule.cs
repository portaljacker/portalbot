using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalBot.Commands
{
    public class AskModule : ModuleBase
    {
        private readonly Random _random;
        private readonly List<string> _answers = new List<string> {
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
        };

        public AskModule(Random random)
        {
            _random = random;
        }

        [Command("ask"), Summary("Ask a question.")]
        public async Task Ask([Remainder, Summary("Your question")] string question)
        {
            var answer = _answers[_random.Next(_answers.Count)];
            var userInfo = Context.User;
            await ReplyAsync($"{userInfo.Username} asked, \"{question}\" Magic 8-ball says: _**\"{answer}\"**_");
        }
    }
}
