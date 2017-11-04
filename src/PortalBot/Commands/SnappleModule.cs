using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Jurassic;
using Jurassic.Library;
using Newtonsoft.Json;
using PortalBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalBot.Commands
{
    [Group("snapple")]
    public class SnappleModule : ModuleBase
    {
        private const string FactUriStem = "http://www.snapple.com/real-facts";

        private readonly Random _random;
        private readonly Dictionary<string, Fact> _facts;
        private readonly ScriptEngine _scriptEngine;

        public SnappleModule(Random random, Dictionary<string, Fact> facts, ScriptEngine scriptEngine)
        {
            _random = random;
            _facts = facts;
            _scriptEngine = scriptEngine;

            if (_facts.Count == 0)
            {
                LoadFacts();
            }
        }

        [Command]
        [Summary("Get a random Snapple \"Real Fact\"")]
        public async Task GetFact()
        {
            var fact = RandomValues(_facts).Take(1).FirstOrDefault();

            var builder = new EmbedBuilder()
                .WithTitle($"\"Real Fact\" #{fact?.Number}")
                .WithDescription(fact?.Data)
                .WithUrl($"https://www.snapple.com/real-facts/{fact?.Number}")
                .WithColor(new Color(0xCA621B))
                .WithAuthor(author =>
                {
                    author
                        .WithName("Snapple")
                        .WithUrl("https://www.snapple.com")
                        .WithIconUrl("https://www.snapple.com/images/ico/Icon-72@2x.png");
                });
            var embed = builder.Build();

            await ReplyAsync("", embed: embed);
        }

        private void LoadFacts()
        {
            var web = new HtmlWeb();
            var doc = web.Load(FactUriStem);
            var script = doc.DocumentNode.SelectNodes("//script")
                .FirstOrDefault(n =>
                    n.InnerText
                    .Contains("pageData"))?.InnerText;

            var result = _scriptEngine.Evaluate("(function() { " + script + " return pageData; })()");
            var json = JSONObject.Stringify(_scriptEngine, result);

            var factDictionary = JsonConvert.DeserializeObject<Dictionary<string, Fact>>(json);

            foreach (var fact in factDictionary)
            {
                _facts.Add(fact.Key, fact.Value);
            }
        }

        private IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var values = dict.Values.ToList();
            var size = dict.Count;
            while (true)
            {
                yield return values[_random.Next(size)];
            }
        }
    }
}
