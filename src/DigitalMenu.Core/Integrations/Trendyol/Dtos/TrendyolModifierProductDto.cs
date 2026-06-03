using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Modifier grubu içindeki seçilebilir opsiyon ürünü.
/// </summary>
public class TrendyolModifierProductDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
