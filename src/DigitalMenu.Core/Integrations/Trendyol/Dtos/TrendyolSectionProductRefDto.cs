using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Kategori içindeki ürün referansı (sıra bilgisi ile).
/// </summary>
public class TrendyolSectionProductRefDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}
