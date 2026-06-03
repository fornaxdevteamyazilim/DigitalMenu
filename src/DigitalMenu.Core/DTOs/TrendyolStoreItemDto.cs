namespace DigitalMenu.Core.DTOs;

/// <summary>
/// AdminPanel'e dönen sadeleştirilmiş Trendyol mağaza bilgisi.
/// </summary>
public class TrendyolStoreItemDto
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? WorkingStatus { get; set; }
    public string? Address { get; set; }
}
