using Microsoft.AspNetCore.SignalR;

namespace DigitalMenu.API.Hubs;

public class MenuHub : Hub
{
    // Restoran yönetim paneli (Kasa/Mutfak) sisteme bağlandığında bu metodu tetikleyecek
    public async Task JoinRestoranGroup(string tenantId)
    {
        // Bağlantıyı restoranın benzersiz grubuna ekliyoruz
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }

    // Restoran yönetim paneli kapandığında veya ayrıldığında
    public async Task LeaveRestoranGroup(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }
}
