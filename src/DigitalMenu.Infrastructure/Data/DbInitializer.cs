using DigitalMenu.Core.Models;
using DigitalMenu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalMenu.Infrastructure.Data;

/// <summary>
/// Uygulama ilk ayağa kalktığında örnek tenant ve ilişkili verileri ekler.
/// </summary>
/// <remarks>
/// Seed aşamasında aktif HTTP isteği olmadığı için <see cref="Core.Services.ITenantProvider"/> boş döner.
/// Bu yüzden tüm <see cref="ITenantEntity"/> kayıtlarına manuel <c>TenantId</c> atanır.
/// Okuma tarafında global tenant filtresini aşmak için <see cref="EntityFrameworkQueryableExtensions.IgnoreQueryFilters{TEntity}(IQueryable{TEntity})"/> kullanılır.
/// <see cref="AppDbContext"/> içindeki <c>ApplyChangeTracking</c>, TenantId zaten set edilmişse üzerine yazmaz.
/// </remarks>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        await context.Database.MigrateAsync();

        var qrBaseUrl = QrMenuUrlBuilder.ResolveBaseUrl(configuration);

        // ITenantProvider boşken global filter devreye girer; tenant var mı kontrolü filtresiz yapılır
        if (!await context.Tenants.IgnoreQueryFilters().AnyAsync())
        {
            var defaultTenant = new Tenant
            {
                TenantId = "lezzet-duragi",
                Name = "Lezzet Durağı Restoranı",
                LogoUrl = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=200",
                PrimaryColor = "#E63946",
                SecondaryColor = "#F1FAEE",
                SelectedTemplate = "Modern",
                PriceMultiplier = 0.85m,
                TrendyolSellerId = "123456",
                TrendyolApiKey = "mock_api_key",
                TrendyolApiSecret = "mock_api_secret"
            };

            await context.Tenants.AddAsync(defaultTenant);
            await context.SaveChangesAsync();

            var burgerCategory = new Category { Name = "Burgerler", DisplayOrder = 1, TenantId = "lezzet-duragi" };
            var drinkCategory = new Category { Name = "İçecekler", DisplayOrder = 2, TenantId = "lezzet-duragi" };
            var dessertCategory = new Category { Name = "Tatlılar", DisplayOrder = 3, TenantId = "lezzet-duragi" };

            await context.Categories.AddRangeAsync(burgerCategory, drinkCategory, dessertCategory);
            await context.SaveChangesAsync();

            var products = new List<Product>
            {
                new()
                {
                    Name = "Klasik Burger",
                    Description = "150gr dana köfte, marul, domates, turşu ve özel sos",
                    OriginalPrice = 220.00m,
                    DisplayPrice = 187.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500",
                    IsAvailable = true,
                    CategoryId = burgerCategory.Id,
                    TenantId = "lezzet-duragi"
                },
                new()
                {
                    Name = "Cheeseburger",
                    Description = "150gr dana köfte, çift cheddar peyniri, karamelize soğan",
                    OriginalPrice = 250.00m,
                    DisplayPrice = 212.50m,
                    ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=500",
                    IsAvailable = true,
                    CategoryId = burgerCategory.Id,
                    TenantId = "lezzet-duragi"
                },
                new()
                {
                    Name = "Coca Cola 330ml",
                    Description = "Kutu içecek",
                    OriginalPrice = 50.00m,
                    DisplayPrice = 42.50m,
                    ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500",
                    IsAvailable = true,
                    CategoryId = drinkCategory.Id,
                    TenantId = "lezzet-duragi"
                },
                new()
                {
                    Name = "Ayran 300ml",
                    Description = "Yıkılan yayık ayranı",
                    OriginalPrice = 35.00m,
                    DisplayPrice = 30.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1625938146369-adc83368bda7?w=500",
                    IsAvailable = true,
                    CategoryId = drinkCategory.Id,
                    TenantId = "lezzet-duragi"
                },
                new()
                {
                    Name = "Sufle",
                    Description = "Pudra şekeri ve vanilyalı dondurma ile",
                    OriginalPrice = 120.00m,
                    DisplayPrice = 102.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=500",
                    IsAvailable = true,
                    CategoryId = dessertCategory.Id,
                    TenantId = "lezzet-duragi"
                }
            };

            await context.Products.AddRangeAsync(products);

            var tables = new[]
            {
                "Masa 1", "Masa 2", "Masa 3", "Bahçe 1", "Bahçe 2"
            }.Select(name => new Table
            {
                TableNumber = name,
                QrCodeUrl = QrMenuUrlBuilder.Build(qrBaseUrl, "lezzet-duragi", name),
                TenantId = "lezzet-duragi"
            }).ToList();

            await context.Tables.AddRangeAsync(tables);
            await context.SaveChangesAsync();
        }

        await RefreshQrCodeUrlsAsync(context, configuration);
    }

    /// <summary>
    /// Production'da QrMenu__BaseUrl set edildiğinde eski localhost QR linklerini günceller.
    /// </summary>
    public static async Task RefreshQrCodeUrlsAsync(AppDbContext context, IConfiguration configuration)
    {
        var qrBaseUrl = QrMenuUrlBuilder.ResolveBaseUrl(configuration);
        if (QrMenuUrlBuilder.IsLocalhostBase(qrBaseUrl))
        {
            Console.WriteLine("--> QR URL güncellemesi atlandı: QrMenu__BaseUrl / Production ayarı yok.");
            return;
        }

        var tables = await context.Tables.IgnoreQueryFilters().ToListAsync();
        var updated = 0;

        foreach (var table in tables)
        {
            if (string.IsNullOrEmpty(table.TenantId))
                continue;

            var newUrl = QrMenuUrlBuilder.Build(qrBaseUrl!, table.TenantId, table.TableNumber);
            if (table.QrCodeUrl == newUrl)
                continue;

            table.QrCodeUrl = newUrl;
            updated++;
        }

        if (updated > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"--> {updated} masa QR linki güncellendi → {qrBaseUrl}");
        }
    }
}
