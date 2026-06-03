using DigitalMenu.Core.Models;

namespace DigitalMenu.Core.Services;

public interface ITableService
{
    Task<Table> CreateTableAsync(string tableNumber);
    Task<List<Table>> GetTablesAsync();
    Task<bool> ToggleWaiterCallAsync(Guid tableId, bool isCalled);
    Task<bool> ToggleBillRequestAsync(Guid tableId, bool isRequested);
}
