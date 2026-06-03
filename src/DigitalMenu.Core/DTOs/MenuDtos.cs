namespace DigitalMenu.Core.DTOs;

public class MenuProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DisplayPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public List<MenuProductOptionDto> RemovableIngredients { get; set; } = [];
    public List<MenuProductOptionDto> ExtraSauces { get; set; } = [];
}

public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public List<MenuProductDto> Products { get; set; } = [];
}

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DisplayPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
