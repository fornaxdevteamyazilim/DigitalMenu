using System.Text.Json.Serialization;
using DigitalMenu.Core.Integrations.Trendyol.Dtos;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// GET .../suppliers/{supplierId}/stores/{storeId}/products yanıt gövdesi.
/// Kategoriler (sections), ürünler, malzemeler ve modifier grupları ayrı koleksiyonlarda gelir.
/// </summary>
public class TrendyolMenuResponse
{
    [JsonPropertyName("sections")]
    public List<TrendyolSectionDto> Sections { get; set; } = [];

    [JsonPropertyName("products")]
    public List<TrendyolProductDto> Products { get; set; } = [];

    [JsonPropertyName("ingredients")]
    public List<TrendyolIngredientDto> Ingredients { get; set; } = [];

    [JsonPropertyName("modifierGroups")]
    public List<TrendyolModifierGroupDto> ModifierGroups { get; set; } = [];
}
