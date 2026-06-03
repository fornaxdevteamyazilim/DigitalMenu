namespace DigitalMenu.Core.DTOs;

public class TrendyolCategoryDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public List<TrendyolProductDto> Items { get; set; } = new();
}

public class TrendyolProductDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool Approved { get; set; } // Ürün Trendyol'da aktif mi?
}
