namespace DigitalMenu.Core.Models;

/// <summary>
/// Trendyol menüsünden çözümlenmiş ürün özelleştirme seçenekleri (JSON olarak saklanır).
/// </summary>
public class ProductOptions
{
    public List<ProductOptionItem> RemovableIngredients { get; set; } = [];
    public List<ProductOptionItem> ExtraSauces { get; set; } = [];
}

public class ProductOptionItem
{
    public string Id { get; set; } = null!;
    public string Label { get; set; } = null!;
    public decimal Price { get; set; }
    public string? GroupName { get; set; }
}
