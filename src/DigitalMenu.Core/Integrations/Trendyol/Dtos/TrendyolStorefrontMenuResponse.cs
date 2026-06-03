using System.Text.Json.Serialization;

namespace DigitalMenu.Core.Integrations.Trendyol.Dtos;

/// <summary>
/// Trendyol GO storefront (tüketici) menü yanıtı.
/// GET /web-restaurant-apirestaurant-santral/restaurants/{storeId}?latitude=&amp;longitude=
/// Yalnızca ürün görsellerini (imageUrl) elde etmek için kullanılır.
/// Resmi olmayan bir uçtur; yapısı değişebilir.
/// </summary>
public class TrendyolStorefrontMenuResponse
{
    [JsonPropertyName("restaurant")]
    public TrendyolStorefrontRestaurant? Restaurant { get; set; }
}

public class TrendyolStorefrontRestaurant
{
    [JsonPropertyName("sections")]
    public List<TrendyolStorefrontSection> Sections { get; set; } = [];
}

public class TrendyolStorefrontSection
{
    [JsonPropertyName("products")]
    public List<TrendyolStorefrontProduct> Products { get; set; } = [];
}

public class TrendyolStorefrontProduct
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}
