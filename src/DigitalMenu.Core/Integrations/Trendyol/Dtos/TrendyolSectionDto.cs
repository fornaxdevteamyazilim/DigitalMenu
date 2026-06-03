using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Menü kategorisi (API'de "section").
/// </summary>
public class TrendyolSectionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>Örn: ACTIVE, PASSIVE</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("products")]
    public List<TrendyolSectionProductRefDto> Products { get; set; } = [];
}
