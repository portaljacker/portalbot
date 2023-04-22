namespace PortalBot.Enums;

using Discord.Interactions;

public enum WeatherUnits
{
    [ChoiceDisplay("CA - Canadian")]
    Ca,
    [ChoiceDisplay("Metric")]
    Metric,
    [ChoiceDisplay("UK - British")]
    Uk,
    [ChoiceDisplay("US - American (Default)")]
    Us
}
