using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Ürün → modifier group ilişki referansı.
/// </summary>
public class TrendyolProductModifierGroupRefDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
