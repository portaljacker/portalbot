namespace PortalBot.Models;

using System.Text.Json.Serialization;

public record GeocoderResponse(
    GeocoderResult[] Results,
    string Status);

public record GeocoderResult(
    string[] Types,
    [property: JsonPropertyName("formatted_address")] string FormattedAddress,
    [property: JsonPropertyName("address_components")] AddressComponent[] AddressComponents,
    bool PartialMatch,
    [property: JsonPropertyName("place_id")] string PlaceId,
    string[] PostCodeLocalities,
    Geometry Geometry);

public record AddressComponent(
    [property: JsonPropertyName("short_name")] string ShortName,
    [property: JsonPropertyName("long_name")] string LongName,
    string[] PostCodeLocalities,
    string[] Types);

public record Geometry(
    LatLng Location,
    [property: JsonPropertyName("location_type")] string LocationType,
    LatLngBounds Viewport,
    LatLngBounds Bounds);

public record LatLng(
    decimal Lat,
    decimal Lng);

public record LatLngBounds(
    LatLng NorthEast,
    LatLng SouthWest);
