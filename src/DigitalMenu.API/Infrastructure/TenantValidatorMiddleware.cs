using DigitalMenu.Core.Services;
using DigitalMenu.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DigitalMenu.API.Infrastructure;

public class TenantValidatorMiddleware
{
    private readonly RequestDelegate _next;

    public TenantValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, AppDbContext dbContext)
    {
        // CORS preflight istekleri tenant doğrulamasına takılmamalı
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Swagger veya Login/Register gibi genel istekleri filtrele dışı bırakabiliriz
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && (
            path.Contains("/swagger")
            || path.Contains("/health")
            || path.Contains("/api/debug")
            || path.Contains("/api/admin")
            || path.Contains("/api/tenants/register")
            || path.StartsWith("/hubs")))
        {
            await _next(context);
            return;
        }

        var tenantId = tenantProvider.GetTenantId();

        if (string.IsNullOrEmpty(tenantId))
        {
            // Eğer sistem çoklu kiracılı zorunlu bir endpoint'e gidiyorsa ve tenant yoksa hata dön
            if (path != null && path.StartsWith("/api/"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header veya tenant parametresi eksik!" });
                return;
            }
        }
        else
        {
            // TenantId var, peki veritabanımızda böyle bir restoran kayıtlı mı?
            // NOT: Global Query Filter Tenant tablosuna işlemez veya IgnoreQueryFilters() ile aşabiliriz.
            var tenantExists = await dbContext.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.TenantId == tenantId && !t.IsDeleted);

            if (!tenantExists)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { error = $"'{tenantId}' adında aktif bir işletme bulunamadı." });
                return;
            }
        }

        await _next(context);
    }
}
