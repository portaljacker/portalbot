namespace PortalBot.Models;

using System.Text.Json.Serialization;

public record Fact(
    [property: JsonPropertyName("n")] string Number,
    [property: JsonPropertyName("d")] string Data
);

// Deserializing category requires making a complicated SingleOrArray JsonConverter
// Example: https://stackoverflow.com/q/59430728
