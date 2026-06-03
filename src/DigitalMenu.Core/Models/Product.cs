namespace DigitalMenu.Core.Models;

public class Product : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal OriginalPrice { get; set; } // Trendyol'dan gelen fiyat
    public decimal DisplayPrice { get; set; }  // Restoranda müşteriye gösterilen (düzenlenmiş) fiyat
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    public string? TrendyolProductId { get; set; } // Trendyol ile senkronizasyon için

    /// <summary>Trendyol ingredients / extraIngredients / modifierGroups → JSON.</summary>
    public string? ProductOptionsJson { get; set; }
}
