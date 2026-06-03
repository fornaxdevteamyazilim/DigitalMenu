using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Trendyol menü ürünü.
/// </summary>
public class TrendyolProductDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Müşteriye yansıyan satış fiyatı (paket/komisyon dahil olabilir).</summary>
    [JsonPropertyName("sellingPrice")]
    public decimal SellingPrice { get; set; }

    [JsonPropertyName("originalPrice")]
    public decimal? OriginalPrice { get; set; }

    [JsonPropertyName("ownSellable")]
    public bool OwnSellable { get; set; }

    /// <summary>Örn: ACTIVE, PASSIVE</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("ingredients")]
    public List<long> Ingredients { get; set; } = [];

    [JsonPropertyName("extraIngredients")]
    public List<long> ExtraIngredients { get; set; } = [];

    [JsonPropertyName("modifierGroups")]
    public List<TrendyolProductModifierGroupRefDto> ModifierGroups { get; set; } = [];
}
