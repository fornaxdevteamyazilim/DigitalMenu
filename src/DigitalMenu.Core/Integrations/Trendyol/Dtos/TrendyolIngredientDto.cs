using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Malzeme / ingredient tanımı (çıkarılabilir veya ekstra eklenebilir).
/// </summary>
public class TrendyolIngredientDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
