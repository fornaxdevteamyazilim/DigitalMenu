using DigitalMenu.Core.Models;

namespace DigitalMenu.Core.Entities;

public abstract class TenantEntity : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = string.Empty;
}
