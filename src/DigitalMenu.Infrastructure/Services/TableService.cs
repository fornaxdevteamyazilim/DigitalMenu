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

    public TableService(
        AppDbContext dbContext,
        ITenantProvider tenantProvider,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _qrMenuBaseUrl = QrMenuUrlBuilder.ResolveBaseUrl(configuration);
    }

    public async Task<Table> CreateTableAsync(string tableNumber)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (string.IsNullOrEmpty(tenantId))
            throw new Exception("İşletme bilgisi doğrulanamadı.");

        var qrCodeUrl = QrMenuUrlBuilder.Build(_qrMenuBaseUrl, tenantId, tableNumber);

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
        var tables = await _dbContext.Tables.OrderBy(t => t.TableNumber).ToListAsync();
        await RepairStoredQrUrlsAsync(tables);
        return tables;
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

    private async Task RepairStoredQrUrlsAsync(List<Table> tables)
    {
        if (QrMenuUrlBuilder.IsLocalhostBase(_qrMenuBaseUrl))
            return;

        var dirty = false;
        foreach (var table in tables)
        {
            if (string.IsNullOrEmpty(table.TenantId))
                continue;

            var correct = QrMenuUrlBuilder.Build(_qrMenuBaseUrl, table.TenantId, table.TableNumber);
            if (table.QrCodeUrl == correct)
                continue;

            table.QrCodeUrl = correct;
            dirty = true;
        }

        if (dirty)
            await _dbContext.SaveChangesAsync();
    }
}
