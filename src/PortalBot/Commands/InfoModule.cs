using Discord.Commands;
using System.Threading.Tasks;

namespace PortalBot.Commands
{
    public class InfoModule : ModuleBase
    {
        [Command("ping")]
        [Summary("pong!")]
        public async Task Ping()
        {
            await ReplyAsync("pong!");
        }

        [Command("say")]
        [Summary("Echos a message.")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            var userInfo = Context.User;
            await ReplyAsync($"{userInfo.Username} wants me to say, \"{echo}\" frankly I think that's a ridiculous expectation on their part, now don't you?");
        }
    }
}
