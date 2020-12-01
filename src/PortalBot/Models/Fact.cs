using System.Text.Json.Serialization;

namespace PortalBot.Models
{

    public class Fact
    {
        [JsonPropertyName("n")] public string Number { get; set; }

        [JsonPropertyName("d")] public string Data { get; set; }

        // Deserializing category requires making a complicated SingleOrArray JsonConverter
        // Example: https://stackoverflow.com/q/59430728
    }
}
