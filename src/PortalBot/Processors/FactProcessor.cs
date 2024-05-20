namespace PortalBot.Processors;

using System.Text.Json;
using Discord;
using Models;

public class FactProcessor
{
    private readonly Dictionary<string, Fact> _facts;
    private readonly Random _random;

    public FactProcessor(Dictionary<string, Fact> facts, Random random)
    {
        _facts = facts;
        _random = random;

        if (_facts.Count == 0)
        {
            LoadFacts();
        }
    }

    private void LoadFacts()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/realFacts.json");
        var json = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var factDictionary = JsonSerializer.Deserialize<Dictionary<string, Fact>>(json);

        if (factDictionary == null)
        {
            return;
        }

        foreach (var (factKey, factValue) in factDictionary)
        {
            _facts.Add(factKey, factValue);
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
            .WithColor(new Color(0x275999))
            .WithAuthor(author => author
                .WithName("Snapple")
                .WithUrl("https://www.snapple.com")
                .WithIconUrl("https://www.snapple.com/images/ico/Icon-72@2x.png"))
            .WithFooter(footer => footer
                .WithText("Disclaimer: Snapple facts may not be entirely factual."));

        return builder.Build();
    }
}
