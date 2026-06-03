using DigitalMenu.Core.Models;

namespace DigitalMenu.Core.DTOs;

public static class MenuProductMapping
{
    public static MenuProductOptionDto ToOptionDto(ProductOptionItem item) =>
        new()
        {
            Id = item.Id,
            Label = item.Label,
            Price = item.Price,
            GroupName = item.GroupName
        };

    public static void ApplyProductOptions(MenuProductDto dto, string? productOptionsJson)
    {
        var options = ProductOptionsJson.Deserialize(productOptionsJson);
        dto.RemovableIngredients = options.RemovableIngredients.Select(ToOptionDto).ToList();
        dto.ExtraSauces = options.ExtraSauces.Select(ToOptionDto).ToList();
    }

    public static ProductOptionItem ToOptionItem(MenuProductOptionDto dto) =>
        new()
        {
            Id = dto.Id,
            Label = dto.Label,
            Price = dto.Price,
            GroupName = dto.GroupName
        };

    public static ProductOptions ToProductOptions(UpdateProductOptionsDto dto) =>
        new()
        {
            RemovableIngredients = dto.RemovableIngredients.Select(ToOptionItem).ToList(),
            ExtraSauces = dto.ExtraSauces.Select(ToOptionItem).ToList()
        };
}
