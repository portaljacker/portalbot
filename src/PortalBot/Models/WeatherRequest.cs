namespace PortalBot.Models;

using PortalBot.Enums;

public record WeatherRequest(
    string Location,
    DarkSkyUnits Units);
