namespace DigitalMenu.Core.Models;

public class Table : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = null!;
    public string TableNumber { get; set; } = null!; // Örn: "Masa 12", "Bahçe-3"
    public string QrCodeUrl { get; set; } = null!;   // Oluşturulan QR kod görsel linki veya yönlendirme URL'si

    // Canlı durum takibi
    public bool IsWaiterCalled { get; set; } = false;
    public bool IsBillRequested { get; set; } = false;
    public DateTime? LastRequestTime { get; set; }
}
