namespace DigitalMenu.Core.DTOs;

public class MenuProductOptionDto
{
    public string Id { get; set; } = null!;
    public string Label { get; set; } = null!;
    public decimal Price { get; set; }
    public string? GroupName { get; set; }
}
