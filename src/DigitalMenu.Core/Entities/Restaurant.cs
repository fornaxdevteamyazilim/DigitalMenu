using DigitalMenu.Core.Enums;
using DigitalMenu.Core.Models;
using DigitalMenu.Core.ValueObjects;

namespace DigitalMenu.Core.Entities;

/// <summary>
/// Kiracıya ait restoran şubesi. Public menü /r/{slug} rotasında slug global benzersizdir.
/// </summary>
public class Restaurant : TenantEntity
{
    public required string Name { get; set; }

    /// <summary>Public menü URL'si için global benzersiz slug.</summary>
    public required string Slug { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public MenuTemplateId TemplateId { get; set; } = MenuTemplateId.Classic;

    public ColorPalette ColorPalette { get; set; } = ColorPalette.Default;

    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
