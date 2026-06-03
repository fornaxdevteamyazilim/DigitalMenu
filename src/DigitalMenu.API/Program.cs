using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DigitalMenu.Core.DTOs;
using DigitalMenu.Core.Models;
using DigitalMenu.Core.Services;
using DigitalMenu.API.Hubs;
using DigitalMenu.API.Infrastructure;
using DigitalMenu.Infrastructure;
using DigitalMenu.Infrastructure.Data;
using DigitalMenu.Infrastructure.Services;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 1. HttpContextAccessor Kaydı (TenantProvider'ın HttpContext'e erişebilmesi için şart)
builder.Services.AddHttpContextAccessor();

// 2. Tenant Provider Kaydı (Her HTTP isteğinde scoped olarak yaratılır)
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

// 3. PostgreSQL ve DbContext Kaydı (ConnectionStrings__DefaultConnection veya Railway DATABASE_URL)
var connectionString = ConnectionStringHelper.Resolve(builder.Configuration)
    ?? throw new InvalidOperationException(
        "Connection string is not configured. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");

builder.Services.AddInfrastructure(connectionString);

// Trendyol Integration Service Kaydı
builder.Services.AddHttpClient<ITrendyolIntegrationService, TrendyolIntegrationService>();

builder.Services.AddScoped<ITableService, TableService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR Kaydı (İleride Garson çağrıları için kullanacağız)
builder.Services.AddSignalR();

// Blazor WASM + QR menü istekleri için CORS (Cors:AllowedOrigins veya CORS_ALLOWED_ORIGINS)
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (corsOrigins is null or { Length: 0 })
{
    var corsEnv = builder.Configuration["CORS_ALLOWED_ORIGINS"];
    corsOrigins = string.IsNullOrWhiteSpace(corsEnv)
        ? null
        : corsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

corsOrigins ??=
[
    "http://localhost:5140",
    "https://localhost:5140",
    "http://localhost:5000",
    "https://localhost:5001",
    "http://localhost:5173",
    "https://localhost:5173",
    "http://localhost:5287"
];

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasm", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseForwardedHeaders();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/debug/qr-config", (IConfiguration config) =>
{
    var baseUrl = QrMenuUrlBuilder.ResolveBaseUrl(config);
    return Results.Ok(new
    {
        qrMenuBaseUrl = baseUrl,
        environment = config["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        configQrMenu = config["QrMenu:BaseUrl"],
        envQrMenu = Environment.GetEnvironmentVariable("QrMenu__BaseUrl")
    });
});

app.UseCors("BlazorWasm");

// 4. Yazdığımız Tenant Doğrulama Middleware'ini Boru Hattına (Pipeline) Ekle
app.UseMiddleware<TenantValidatorMiddleware>();

// Örnek Bir Minimal API Endpoint'i (Test etmek için)
// Bu sorguda ürünleri çekerken ASLA 'where x.TenantId == ...' yazmıyoruz, EF Core arkada otomatik ekliyor!
app.MapGet("/api/products", async (AppDbContext dbContext) =>
{
    var products = await dbContext.Products
        .OrderBy(p => p.Name)
        .Select(p => new ProductListItemDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            Name = p.Name,
            Description = p.Description,
            OriginalPrice = p.OriginalPrice,
            DisplayPrice = p.DisplayPrice,
            ImageUrl = p.ImageUrl,
            IsAvailable = p.IsAvailable
        })
        .ToListAsync();
    return Results.Ok(products);
});

app.MapGet("/api/products-by-categories", async (AppDbContext dbContext) =>
{
    var categories = await dbContext.Categories
        .OrderBy(c => c.DisplayOrder)
        .Include(c => c.Products)
        .ToListAsync();

    var result = categories.Select(c => new MenuCategoryDto
    {
        Id = c.Id,
        Name = c.Name,
        DisplayOrder = c.DisplayOrder,
        Products = c.Products
            .OrderBy(p => p.Name)
            .Select(p =>
            {
                var dto = new MenuProductDto
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    DisplayPrice = p.DisplayPrice,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = p.IsAvailable
                };
                MenuProductMapping.ApplyProductOptions(dto, p.ProductOptionsJson);
                return dto;
            })
            .ToList()
    }).ToList();

    return Results.Ok(result);
});

app.MapPost("/api/products/update-price", async (UpdateProductPriceDto dto, AppDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(dto.Id);
    if (product is null) return Results.NotFound();

    product.DisplayPrice = dto.DisplayPrice;
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/products/toggle-availability", async (ToggleProductAvailabilityDto dto, AppDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(dto.Id);
    if (product is null) return Results.NotFound();

    product.IsAvailable = dto.IsAvailable;
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/api/products/{productId:guid}/options", async (Guid productId, UpdateProductOptionsDto dto, AppDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(productId);
    if (product is null) return Results.NotFound();

    var options = MenuProductMapping.ToProductOptions(dto);
    product.ProductOptionsJson = ProductOptionsJson.Serialize(options);
    await dbContext.SaveChangesAsync();

    var response = new MenuProductDto
    {
        Id = product.Id,
        CategoryId = product.CategoryId,
        Name = product.Name
    };
    MenuProductMapping.ApplyProductOptions(response, product.ProductOptionsJson);
    return Results.Ok(new
    {
        response.RemovableIngredients,
        response.ExtraSauces
    });
});

app.MapPut("/api/products/{productId:guid}/display-price", async (Guid productId, UpdateDisplayPriceDto dto, AppDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(productId);
    if (product is null) return Results.NotFound();

    product.DisplayPrice = dto.DisplayPrice;
    await dbContext.SaveChangesAsync();
    return Results.Ok(new { product.Id, product.DisplayPrice });
});

app.MapPost("/api/integration/trendyol-sync", async (ITrendyolIntegrationService integrationService, ITenantProvider tenantProvider) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId))
    {
        return Results.BadRequest("İşlem yapılacak restoran belirlenemedi.");
    }

    var result = await integrationService.SyncMenuAsync(tenantId);
    if (!result)
    {
        return Results.BadRequest("Trendyol senkronizasyonu başarısız oldu. Lütfen API anahtarlarınızı kontrol edin.");
    }

    return Results.Ok(new { message = "Menünüz başarıyla Trendyol'dan aktarıldı ve güncellendi." });
});

// Trendyol GO Mağaza/Restoran Listesini Getir
app.MapGet("/api/integration/trendyol-stores", async (
    ITrendyolIntegrationService integrationService,
    ITenantProvider tenantProvider) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId)) return Results.BadRequest("İşletme doğrulanamadı.");

    var stores = await integrationService.GetStoresAsync(tenantId);
    return Results.Ok(stores);
});

// Trendyol Entegrasyon Ayarlarını Getir
app.MapGet("/api/integration/trendyol-settings", async (ITenantProvider tenantProvider, AppDbContext dbContext) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId)) return Results.BadRequest("İşletme doğrulanamadı.");

    var tenant = await dbContext.Tenants
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(t => t.TenantId == tenantId && !t.IsDeleted);
    if (tenant == null) return Results.NotFound();

    return Results.Ok(new TrendyolSettingsDto
    {
        TrendyolSellerId = tenant.TrendyolSellerId,
        TrendyolStoreId = tenant.TrendyolStoreId,
        TrendyolApiKey = tenant.TrendyolApiKey,
        TrendyolApiSecret = tenant.TrendyolApiSecret,
        TrendyolAgentName = tenant.TrendyolAgentName,
        TrendyolExecutorUser = tenant.TrendyolExecutorUser,
        TrendyolHost = tenant.TrendyolHost,
        PriceMultiplier = tenant.PriceMultiplier
    });
});

// Trendyol Entegrasyon Ayarlarını Kaydet
app.MapPost("/api/integration/trendyol-settings", async (
    TrendyolSettingsDto dto,
    ITenantProvider tenantProvider,
    AppDbContext dbContext) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId)) return Results.BadRequest("İşletme doğrulanamadı.");

    var tenant = await dbContext.Tenants
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(t => t.TenantId == tenantId && !t.IsDeleted);
    if (tenant == null) return Results.NotFound();

    tenant.TrendyolSellerId = dto.TrendyolSellerId;
    tenant.TrendyolStoreId = dto.TrendyolStoreId;
    tenant.TrendyolApiKey = dto.TrendyolApiKey;
    tenant.TrendyolApiSecret = dto.TrendyolApiSecret;
    tenant.TrendyolAgentName = dto.TrendyolAgentName;
    tenant.TrendyolExecutorUser = dto.TrendyolExecutorUser;
    tenant.TrendyolHost = string.IsNullOrWhiteSpace(dto.TrendyolHost) ? "https://api.tgoapis.com" : dto.TrendyolHost.Trim();
    // 0 gönderilirse (eksik JSON / bağlama hatası) mevcut çarpanı koru; yalnızca geçerli değer yaz
    if (dto.PriceMultiplier > 0)
    {
        tenant.PriceMultiplier = dto.PriceMultiplier;
    }

    await dbContext.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

// Masaları Listele
app.MapGet("/api/tables", async (ITableService tableService) =>
{
    var tables = await tableService.GetTablesAsync();
    return Results.Ok(tables);
});

// Garson Çağır / İptal Et Endpoint'i
app.MapPost("/api/notifications/waiter-call", async (
    Guid tableId,
    bool isCalled,
    ITableService tableService,
    ITenantProvider tenantProvider,
    IHubContext<MenuHub> hubContext) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId)) return Results.BadRequest("İşletme doğrulanamadı.");

    var success = await tableService.ToggleWaiterCallAsync(tableId, isCalled);
    if (!success) return Results.NotFound("Masa bulunamadı.");

    var tables = await tableService.GetTablesAsync();
    var targetTable = tables.FirstOrDefault(t => t.Id == tableId);

    await hubContext.Clients.Group(tenantId).SendAsync("ReceiveWaiterCall", new
    {
        tableId,
        tableNumber = targetTable?.TableNumber,
        isWaiterCalled = isCalled,
        message = isCalled
            ? $"{targetTable?.TableNumber} garson çağırıyor!"
            : $"{targetTable?.TableNumber} çağrısı iptal edildi."
    });

    return Results.Ok(new { success = true });
});

// Hesap İste / İptal Et Endpoint'i
app.MapPost("/api/notifications/bill-request", async (
    Guid tableId,
    bool isRequested,
    ITableService tableService,
    ITenantProvider tenantProvider,
    IHubContext<MenuHub> hubContext) =>
{
    var tenantId = tenantProvider.GetTenantId();
    if (string.IsNullOrEmpty(tenantId)) return Results.BadRequest("İşletme doğrulanamadı.");

    var success = await tableService.ToggleBillRequestAsync(tableId, isRequested);
    if (!success) return Results.NotFound("Masa bulunamadı.");

    var tables = await tableService.GetTablesAsync();
    var targetTable = tables.FirstOrDefault(t => t.Id == tableId);

    await hubContext.Clients.Group(tenantId).SendAsync("ReceiveBillRequest", new
    {
        tableId,
        tableNumber = targetTable?.TableNumber,
        isBillRequested = isRequested,
        message = isRequested
            ? $"{targetTable?.TableNumber} hesap istiyor!"
            : $"{targetTable?.TableNumber} hesap isteği kapatıldı."
    });

    return Results.Ok(new { success = true });
});

// Yeni Masa Ekle (QR URL otomatik arkada üretilir)
app.MapPost("/api/tables", async (string tableNumber, ITableService tableService) =>
{
    if (string.IsNullOrWhiteSpace(tableNumber))
        return Results.BadRequest("Masa numarası boş olamaz.");

    try
    {
        var newTable = await tableService.CreateTableAsync(tableNumber);
        return Results.Ok(newTable);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// SignalR Endpoint Eşleştirmesi
app.MapHub<MenuHub>("/hubs/menu").RequireCors("BlazorWasm");

// Uygulama ayağa kalkarken Seed Data motorunu çalıştır
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        await DbInitializer.SeedAsync(context, configuration);
        Console.WriteLine("--> Veritabanı kontrol edildi; seed / QR URL güncellemesi tamamlandı.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Seed data yüklenirken hata oluştu: {ex.Message}");
    }
}

app.Run();
