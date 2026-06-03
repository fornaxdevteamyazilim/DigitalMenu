using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// GET integrator/store/meal/suppliers/{supplierId}/stores yanıt gövdesi.
/// </summary>
public class TrendyolStoresResponse
{
    [JsonPropertyName("restaurants")]
    public List<TrendyolStoreApiDto> Restaurants { get; set; } = [];
}

public class TrendyolStoreApiDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("workingStatus")]
    public string? WorkingStatus { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("location")]
    public TrendyolStoreLocation? Location { get; set; }
}

public class TrendyolStoreLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
