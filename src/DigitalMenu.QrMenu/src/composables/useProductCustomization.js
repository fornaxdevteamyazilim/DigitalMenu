/**
 * API'den gelen Trendyol kaynaklı ürün seçenekleri (removableIngredients / extraSauces).
 */
export function getProductCustomization(product) {
  if (!product) {
    return { removableIngredients: [], extraSauces: [] }
  }

  return {
    removableIngredients: product.removableIngredients ?? [],
    extraSauces: product.extraSauces ?? [],
  }
}
