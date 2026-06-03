using DigitalMenu.Core.Models;
using DigitalMenu.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalMenu.Infrastructure.Services;

public class TableService : ITableService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly string _qrMenuBaseUrl;

    public TableService(AppDbContext dbContext, ITenantProvider tenantProvider, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _qrMenuBaseUrl = (configuration["QrMenu:BaseUrl"] ?? "http://localhost:5173/r").TrimEnd('/');
    }

    public async Task<Table> CreateTableAsync(string tableNumber)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
            throw new Exception("İşletme bilgisi doğrulanamadı.");

        var qrCodeUrl = $"{_qrMenuBaseUrl}/{tenantId}?table={Uri.EscapeDataString(tableNumber)}";

        var table = new Table
        {
            TableNumber = tableNumber,
            QrCodeUrl = qrCodeUrl,
            TenantId = tenantId
        };

        _dbContext.Tables.Add(table);
        await _dbContext.SaveChangesAsync();

        return table;
    }

    public async Task<List<Table>> GetTablesAsync()
    {
        return await _dbContext.Tables.OrderBy(t => t.TableNumber).ToListAsync();
    }

    public async Task<bool> ToggleWaiterCallAsync(Guid tableId, bool isCalled)
    {
        var table = await _dbContext.Tables.FindAsync(tableId);
        if (table == null) return false;

        table.IsWaiterCalled = isCalled;
        table.LastRequestTime = isCalled ? DateTime.UtcNow : table.LastRequestTime;

        _dbContext.Tables.Update(table);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleBillRequestAsync(Guid tableId, bool isRequested)
    {
        var table = await _dbContext.Tables.FindAsync(tableId);
        if (table == null) return false;

        table.IsBillRequested = isRequested;
        table.LastRequestTime = isRequested ? DateTime.UtcNow : table.LastRequestTime;

        _dbContext.Tables.Update(table);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
