namespace PortalBot.Models;

using Enums;

public record WeatherRequest(
    string Location,
    WeatherUnits Units);
