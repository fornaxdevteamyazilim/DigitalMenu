using DigitalMenu.Core.DTOs;

namespace DigitalMenu.Core.Services;

public interface ITrendyolIntegrationService
{
    /// <summary>
    /// İlgili restorana ait Trendyol menüsünü çeker ve yerel veritabanına aktarır.
    /// </summary>
    Task<bool> SyncMenuAsync(string tenantId);

    /// <summary>
    /// Tenant'ın Trendyol GO supplier'ına bağlı mağaza/restoran listesini getirir.
    /// </summary>
    Task<List<TrendyolStoreItemDto>> GetStoresAsync(string tenantId);
}
