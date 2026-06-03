using DigitalMenu.Core.Services;

namespace DigitalMenu.API.Infrastructure;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // 1. Strateji: HTTP Header kontrolü (Yönetim Paneli / Admin istekleri için)
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdFromHeader))
        {
            return tenantIdFromHeader.ToString();
        }

        // 2. Strateji: Query String kontrolü (QR Menü istekleri için - örn: ?tenant=kebapci-ali)
        if (httpContext.Request.Query.TryGetValue("tenant", out var tenantIdFromQuery))
        {
            return tenantIdFromQuery.ToString();
        }

        // 3. Strateji: Route Data kontrolü (Eğer URL yapısını /r/{tenant} yaparsanız)
        if (httpContext.Request.RouteValues.TryGetValue("tenant", out var tenantIdFromRoute))
        {
            return tenantIdFromRoute?.ToString();
        }

        return null;
    }
}
