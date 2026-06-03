namespace DigitalMenu.Core.Models;

public class Category : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; } // Menüdeki sıralama
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
