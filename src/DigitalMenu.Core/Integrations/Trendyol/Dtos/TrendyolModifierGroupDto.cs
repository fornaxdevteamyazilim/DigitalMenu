using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Ürün opsiyon/modifier grubu (örn: "Boy Seçimi", "Ekstra Malzemeler").
/// </summary>
public class TrendyolModifierGroupDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("min")]
    public int Min { get; set; }

    [JsonPropertyName("max")]
    public int Max { get; set; }

    [JsonPropertyName("modifierProducts")]
    public List<TrendyolModifierProductDto> ModifierProducts { get; set; } = [];
}
