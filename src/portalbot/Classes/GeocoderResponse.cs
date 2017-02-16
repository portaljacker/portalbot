using Newtonsoft.Json;

namespace portalbot.Classes
{
    public class GeocoderResponse
    {
        public GeocoderResult Results { get; set; }

        public string Status { get; set; }
    }

    public class GeocoderResult
    {
        public string[] Types { get; set; }

        [JsonProperty(PropertyName = "formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty(PropertyName = "address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonProperty(PropertyName = "partial_match")]
        public bool PartialMatch { get; set; }

        [JsonProperty(PropertyName = "place_id")]
        public string PlaceId { get; set; }

        [JsonProperty(PropertyName = "postcode_localities")]
        public string[] PostCodeLocalities { get; set; }

        public Geometry Geometry { get; set; }
    }

    public class AddressComponent
    {
        [JsonProperty(PropertyName = "short_name")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "long_name")]
        public string LongName { get; set; }

        [JsonProperty(PropertyName = "postcode_localities")]
        public string[] PostalCodeLocalities { get; set; }

        public string[] Types { get; set; }
    }

    public class Geometry
    {
        public LatLng Location { get; set; }

        [JsonProperty(PropertyName = "location_type")]
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
