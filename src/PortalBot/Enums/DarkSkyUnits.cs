namespace PortalBot.Enums;

using Discord.Interactions;

public enum DarkSkyUnits
{
    [ChoiceDisplay("Automatic (based on geographic region)")]
    Auto,
    [ChoiceDisplay("Canadian")]
    Ca,
    [ChoiceDisplay("British")]
    Uk2,
    [ChoiceDisplay("American")]
    Us,
    [ChoiceDisplay("Scientific")]
    Si
};
