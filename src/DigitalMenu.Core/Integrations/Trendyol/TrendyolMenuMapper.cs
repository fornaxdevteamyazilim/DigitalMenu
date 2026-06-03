using DigitalMenu.Core.Integrations.Trendyol.Dtos;
using DigitalMenu.Core.Models;

namespace DigitalMenu.Core.Integrations.Trendyol;

/// <summary>
/// Trendyol menü DTO'larını domain modellerine map etmek için yardımcılar.
/// </summary>
public static class TrendyolMenuMapper
{
    public static bool IsActive(string? status) =>
        string.Equals(status, TrendyolMenuStatuses.Active, StringComparison.OrdinalIgnoreCase);

    public static string ToExternalId(long trendyolId) => trendyolId.ToString();

    /// <summary>
    /// Section + product referanslarından kategori → ürün ağacı oluşturur.
    /// </summary>
    public static IEnumerable<(TrendyolSectionDto Section, TrendyolProductDto Product)> FlattenSectionProducts(
        TrendyolMenuResponse menu)
    {
        var productLookup = menu.Products.ToDictionary(p => p.Id);

        foreach (var section in menu.Sections.OrderBy(s => s.Position))
        {
            foreach (var productRef in section.Products.OrderBy(p => p.Position))
            {
                if (productLookup.TryGetValue(productRef.Id, out var product))
                {
                    yield return (section, product);
                }
            }
        }
    }

    /// <summary>
    /// Ürünün ingredients, extraIngredients ve modifierGroups referanslarını menü kataloğundan çözümler.
    /// </summary>
    public static ProductOptions MapProductOptions(
        TrendyolProductDto product,
        TrendyolMenuResponse menu,
        decimal priceMultiplier)
    {
        var activeIngredients = menu.Ingredients
            .Where(i => IsActive(i.Status))
            .ToDictionary(i => i.Id);

        var productLookup = menu.Products.ToDictionary(p => p.Id);
        var modifierGroupLookup = menu.ModifierGroups.ToDictionary(g => g.Id);

        var removable = product.Ingredients
            .Where(activeIngredients.ContainsKey)
            .Select(id => new ProductOptionItem
            {
                Id = ToExternalId(id),
                Label = activeIngredients[id].Name,
                Price = 0
            })
            .ToList();

        var extras = new List<ProductOptionItem>();

        foreach (var extraId in product.ExtraIngredients)
        {
            if (!activeIngredients.TryGetValue(extraId, out var ingredient)) continue;

            extras.Add(new ProductOptionItem
            {
                Id = ToExternalId(extraId),
                Label = ingredient.Name,
                Price = RoundPrice(ingredient.Price * priceMultiplier),
                GroupName = "Ekstra Malzeme"
            });
        }

        foreach (var groupRef in product.ModifierGroups.OrderBy(g => g.Position))
        {
            if (!modifierGroupLookup.TryGetValue(groupRef.Id, out var group)) continue;

            foreach (var modifier in group.ModifierProducts.OrderBy(m => m.Position))
            {
                var label = productLookup.TryGetValue(modifier.Id, out var modifierProduct)
                    ? modifierProduct.Name
                    : $"Seçenek {modifier.Id}";

                extras.Add(new ProductOptionItem
                {
                    Id = $"{group.Id}_{modifier.Id}",
                    Label = label,
                    Price = RoundPrice(modifier.Price * priceMultiplier),
                    GroupName = group.Name
                });
            }
        }

        return new ProductOptions
        {
            RemovableIngredients = removable,
            ExtraSauces = extras
        };
    }

    private static decimal RoundPrice(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
