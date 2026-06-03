namespace DigitalMenu.API.Infrastructure;

public static class TenantValidatorMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantValidatorMiddleware>();
    }
}
