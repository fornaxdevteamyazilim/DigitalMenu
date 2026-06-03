using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using DigitalMenu.Core.DTOs;
using DigitalMenu.Core.Integrations.Trendyol;
using DigitalMenu.Core.Integrations.Trendyol.Dtos;
using DigitalMenu.Core.Models;
using DigitalMenu.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalMenu.Infrastructure.Services;

public class TrendyolIntegrationService : ITrendyolIntegrationService
{
    private const string DefaultHost = "https://api.tgoapis.com";

    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;

    public TrendyolIntegrationService(HttpClient httpClient, AppDbContext dbContext)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    public async Task<bool> SyncMenuAsync(string tenantId)
    {
        // Tenant entegrasyon bilgilerini global filtreyi aşarak çekiyoruz.
        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && !t.IsDeleted);

        if (tenant == null
            || string.IsNullOrWhiteSpace(tenant.TrendyolApiKey)
            || string.IsNullOrWhiteSpace(tenant.TrendyolApiSecret)
            || string.IsNullOrWhiteSpace(tenant.TrendyolSellerId)
            || string.IsNullOrWhiteSpace(tenant.TrendyolStoreId))
        {
            return false; // Entegrasyon bilgileri eksik (supplierId, storeId, apiKey, apiSecret zorunlu)
        }

        var host = BuildHost(tenant);

        // Trendyol GO (Yemek) menü çekme endpoint'i
        var requestUrl = $"{host}/integrator/product/meal/suppliers/{tenant.TrendyolSellerId}/stores/{tenant.TrendyolStoreId}/products";

        using var request = BuildAuthorizedRequest(tenant, HttpMethod.Get, requestUrl);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;

        var menu = await response.Content.ReadFromJsonAsync<TrendyolMenuResponse>();
        if (menu == null) return false;

        // Ayarlar kaydedildikten hemen sonra senkron çağrılabilir; çarpanı DB'den taze oku
        var priceMultiplier = await GetPriceMultiplierAsync(tenant.TenantId);
        await SyncMenuToDatabaseAsync(tenant, menu, priceMultiplier);

        // Görsel zenginleştirme (storefront API). En iyi çaba; hata olursa menü senkronu etkilenmez.
        try
        {
            await EnrichProductImagesAsync(tenant);
        }
        catch
        {
            // Storefront resmi olmayan bir uç; başarısız olursa görseller boş kalır, senkron başarılı sayılır.
        }

        return true;
    }

    public async Task<List<TrendyolStoreItemDto>> GetStoresAsync(string tenantId)
    {
        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && !t.IsDeleted);

        if (tenant == null
            || string.IsNullOrWhiteSpace(tenant.TrendyolApiKey)
            || string.IsNullOrWhiteSpace(tenant.TrendyolApiSecret)
            || string.IsNullOrWhiteSpace(tenant.TrendyolSellerId))
        {
            // supplierId, apiKey, apiSecret zorunlu (storeId gerekmez)
            return [];
        }

        var stores = await GetStoreApiListAsync(tenant);
        return stores.Select(store => new TrendyolStoreItemDto
        {
            Id = store.Id.ToString(),
            Name = store.Name,
            WorkingStatus = store.WorkingStatus,
            Address = store.Address
        }).ToList();
    }

    private async Task<List<TrendyolStoreApiDto>> GetStoreApiListAsync(Tenant tenant)
    {
        var host = BuildHost(tenant);
        var result = new List<TrendyolStoreApiDto>();
        var page = 0;
        const int size = 50;

        while (true)
        {
            var url = $"{host}/integrator/store/meal/suppliers/{tenant.TrendyolSellerId}/stores?page={page}&size={size}";
            using var request = BuildAuthorizedRequest(tenant, HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) break;

            var payload = await response.Content.ReadFromJsonAsync<TrendyolStoresResponse>();
            var restaurants = payload?.Restaurants ?? [];
            if (restaurants.Count == 0) break;

            result.AddRange(restaurants);

            if (restaurants.Count < size) break;
            page++;
        }

        return result;
    }

    /// <summary>
    /// Storefront (tüketici) menüsünden ürün görsellerini çekip eşleşen ürünlerin ImageUrl'ünü doldurur.
    /// Ürün id'leri satıcı API'si ile aynı olduğu için doğrudan eşlenir.
    /// </summary>
    private async Task EnrichProductImagesAsync(Tenant tenant)
    {
        // Storefront isteği için mağaza koordinatları gerekiyor; satıcı mağaza listesinden alıyoruz.
        var stores = await GetStoreApiListAsync(tenant);
        var store = stores.FirstOrDefault(s => s.Id.ToString() == tenant.TrendyolStoreId);
        if (store?.Location == null) return;

        var host = BuildHost(tenant);
        var lat = store.Location.Latitude.ToString(CultureInfo.InvariantCulture);
        var lng = store.Location.Longitude.ToString(CultureInfo.InvariantCulture);
        var url = $"{host}/web-restaurant-apirestaurant-santral/restaurants/{tenant.TrendyolStoreId}?latitude={lat}&longitude={lng}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var correlationId = Guid.NewGuid().ToString();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-correlationid", correlationId);
        request.Headers.Add("pid", correlationId);
        request.Headers.Add("sid", correlationId);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return;

        var storefront = await response.Content.ReadFromJsonAsync<TrendyolStorefrontMenuResponse>();
        var sections = storefront?.Restaurant?.Sections;
        if (sections == null || sections.Count == 0) return;

        // productId -> imageUrl sözlüğü
        var imageMap = new Dictionary<string, string>();
        foreach (var section in sections)
        {
            foreach (var product in section.Products)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    imageMap[product.Id.ToString()] = product.ImageUrl!;
                }
            }
        }

        if (imageMap.Count == 0) return;

        var externalIds = imageMap.Keys.ToList();
        var products = await _dbContext.Products
            .Where(p => p.TrendyolProductId != null && externalIds.Contains(p.TrendyolProductId))
            .ToListAsync();

        var changed = false;
        foreach (var product in products)
        {
            if (product.TrendyolProductId != null && imageMap.TryGetValue(product.TrendyolProductId, out var imageUrl)
                && product.ImageUrl != imageUrl)
            {
                product.ImageUrl = imageUrl;
                changed = true;
            }
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync();
        }
    }

    private static string BuildHost(Tenant tenant) =>
        string.IsNullOrWhiteSpace(tenant.TrendyolHost) ? DefaultHost : tenant.TrendyolHost.TrimEnd('/');

    private static HttpRequestMessage BuildAuthorizedRequest(Tenant tenant, HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{tenant.TrendyolApiKey}:{tenant.TrendyolApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-agentname", tenant.TrendyolAgentName ?? string.Empty);
        request.Headers.Add("x-executor-user", tenant.TrendyolExecutorUser ?? string.Empty);
        return request;
    }

    private async Task<decimal> GetPriceMultiplierAsync(string tenantId)
    {
        var multiplier = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && !t.IsDeleted)
            .Select(t => t.PriceMultiplier)
            .FirstOrDefaultAsync();

        return multiplier > 0 ? multiplier : 1.0m;
    }

    private async Task SyncMenuToDatabaseAsync(Tenant tenant, TrendyolMenuResponse menu, decimal priceMultiplier)
    {
        // Aynı ürün birden fazla section'da görünebilir; ilk göründüğü kategoriye bağlarız.
        var processedProductIds = new HashSet<long>();

        foreach (var (section, product) in TrendyolMenuMapper.FlattenSectionProducts(menu))
        {
            var categoryName = section.Name;

            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    DisplayOrder = section.Position,
                    TenantId = tenant.TenantId
                };
                _dbContext.Categories.Add(category);
                await _dbContext.SaveChangesAsync(); // Id oluşması için
            }
            else if (category.DisplayOrder != section.Position)
            {
                category.DisplayOrder = section.Position;
            }

            if (!processedProductIds.Add(product.Id))
            {
                continue; // Bu ürün daha önce işlendi
            }

            var externalId = TrendyolMenuMapper.ToExternalId(product.Id);
            var displayPrice = Math.Round(product.SellingPrice * priceMultiplier, 2, MidpointRounding.AwayFromZero);
            var isAvailable = TrendyolMenuMapper.IsActive(product.Status);
            var productOptions = TrendyolMenuMapper.MapProductOptions(product, menu, priceMultiplier);
            var productOptionsJson = ProductOptionsJson.Serialize(productOptions);

            var existing = await _dbContext.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p =>
                    p.TrendyolProductId == externalId
                    && p.TenantId == tenant.TenantId
                    && !p.IsDeleted);

            if (existing == null)
            {
                _dbContext.Products.Add(new Product
                {
                    Name = product.Name,
                    Description = product.Description,
                    OriginalPrice = product.SellingPrice,
                    DisplayPrice = displayPrice,
                    ImageUrl = product.ImageUrl,
                    IsAvailable = isAvailable,
                    TrendyolProductId = externalId,
                    ProductOptionsJson = productOptionsJson,
                    CategoryId = category.Id,
                    TenantId = tenant.TenantId
                });
            }
            else
            {
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.OriginalPrice = product.SellingPrice;
                existing.DisplayPrice = displayPrice;
                if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    existing.ImageUrl = product.ImageUrl;
                }
                existing.IsAvailable = isAvailable;
                existing.CategoryId = category.Id;
                existing.ProductOptionsJson = productOptionsJson;
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}
