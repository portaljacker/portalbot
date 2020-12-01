using System.Text.Json.Serialization;

namespace PortalBot.Models
{
    public class GeocoderResponse
    {
        public GeocoderResult[] Results { get; set; }

        public string Status { get; set; }
    }

    public class GeocoderResult
    {
        public string[] Types { get; set; }

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        public bool PartialMatch { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }

        public string[] PostCodeLocalities { get; set; }

        public Geometry Geometry { get; set; }
    }

    public class AddressComponent
    {
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("long_name")]
        public string LongName { get; set; }

        public string[] PostCodeLocalities { get; set; }

        public string[] Types { get; set; }
    }

    public class Geometry
    {
        public LatLng Location { get; set; }

        [JsonPropertyName("location_type")]
        public string LocationType { get; set; }

        public LatLngBounds Viewport { get; set; }

        public LatLngBounds Bounds { get; set; }
    }

    public class LatLng
    {
        public decimal Lat { get; set; }

        public decimal Lng { get; set; }
    }

    public class LatLngBounds
    {
        public LatLng NorthEast { get; set; }

        public LatLng SouthWest { get; set; }
    }
}
