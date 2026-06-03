namespace DigitalMenu.Core.DTOs;

public class TrendyolSettingsDto
{
    public string? TrendyolSellerId { get; set; }   // supplierId
    public string? TrendyolStoreId { get; set; }     // store/restaurant id
    public string? TrendyolApiKey { get; set; }
    public string? TrendyolApiSecret { get; set; }
    public string? TrendyolAgentName { get; set; }
    public string? TrendyolExecutorUser { get; set; }
    public string? TrendyolHost { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;
}
