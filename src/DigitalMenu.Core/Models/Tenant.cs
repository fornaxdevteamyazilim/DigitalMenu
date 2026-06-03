namespace DigitalMenu.Core.Models;

public class Tenant : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!; // URL'de görünecek kısa ad (örn: "kebapci-ali")
    public string Name { get; set; } = null!;     // Restoran tam adı
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }      // Seçtiği renk paleti ana rengi
    public string? SecondaryColor { get; set; }    // Seçtiği renk paleti ikincil rengi
    public string? SelectedTemplate { get; set; }  // "Modern", "Classic" vb.

    // Trendyol GO (Yemek) Entegrasyon Bilgileri
    public string? TrendyolApiKey { get; set; }
    public string? TrendyolApiSecret { get; set; }
    public string? TrendyolSellerId { get; set; }   // Trendyol GO supplierId (örn: 772284)
    public string? TrendyolStoreId { get; set; }     // Trendyol GO mağaza/restoran id (örn: 191699)
    public string? TrendyolAgentName { get; set; }   // x-agentname başlığı
    public string? TrendyolExecutorUser { get; set; } // x-executor-user başlığı
    public string? TrendyolHost { get; set; }         // Varsayılan: https://api.tgoapis.com

    // Esneklik için paket fiyatı çarpanı (örn: %10 indirim için 0.90)
    public decimal PriceMultiplier { get; set; } = 1.0m;
}
