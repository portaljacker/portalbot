namespace PortalBot.Processors;

using System.Text.Json;
using Discord;
using HtmlAgilityPack;
using Jurassic;
using Jurassic.Library;
using PortalBot.Models;

public class FactProcessor
{
    private const string FactUriStem = "http://www.snapple.com/real-facts";

    private readonly Dictionary<string, Fact> _facts;
    private readonly ScriptEngine _scriptEngine;
    private readonly Random _random;

    public FactProcessor(Dictionary<string, Fact> facts, ScriptEngine scriptEngine, Random random)
    {
        _facts = facts;
        _scriptEngine = scriptEngine;
        _random = random;

        if (_facts.Count == 0)
        {
            LoadFacts();
        }
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

        if (json is string s)
        {
            var factDictionary = JsonSerializer.Deserialize<Dictionary<string, Fact>>(s);

            if (factDictionary != null)
            {
                foreach (var (factKey, factValue) in factDictionary)
                {
                    _facts.Add(factKey, factValue);
                }
            }
        }
    }

    public Embed GetFact()
    {
        var fact = RandomFact();

        return CreateEmbed(fact);
    }

    private Fact RandomFact()
    {
        var values = _facts.Values.ToArray();
        var size = _facts.Count;

        return values[_random.Next(size)];
    }

    private static Embed CreateEmbed(Fact? fact)
    {
        var builder = new EmbedBuilder()
            .WithTitle($"\"Real Fact\" #{fact?.Number}")
            .WithDescription(fact?.Data)
            .WithUrl($"https://www.snapple.com/real-facts/{fact?.Number}")
            .WithColor(new Color(0xCA621B))
            .WithAuthor(author => author
                    .WithName("Snapple")
                    .WithUrl("https://www.snapple.com")
                    .WithIconUrl("https://www.snapple.com/images/ico/Icon-72@2x.png"));

        return builder.Build();
    }
}
