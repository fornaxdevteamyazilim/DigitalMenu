namespace DigitalMenu.Core.DTOs;

public class UpdateProductOptionsDto
{
    public List<MenuProductOptionDto> RemovableIngredients { get; set; } = [];
    public List<MenuProductOptionDto> ExtraSauces { get; set; } = [];
}
