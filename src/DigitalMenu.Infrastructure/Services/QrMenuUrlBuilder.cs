using Microsoft.Extensions.Configuration;

namespace DigitalMenu.Infrastructure.Services;

internal static class QrMenuUrlBuilder
{
    public static string Build(string baseUrl, string tenantId, string tableNumber)
    {
        return $"{baseUrl.TrimEnd('/')}/{tenantId}?table={Uri.EscapeDataString(tableNumber)}";
    }

    public static bool IsLocalhostBase(string? baseUrl) =>
        string.IsNullOrWhiteSpace(baseUrl)
        || baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
        || baseUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

    public static bool IsDevelopmentEnvironment(IConfiguration configuration) =>
        string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// QrMenu:BaseUrl veya Railway QrMenu__BaseUrl.
    /// </summary>
    public static string ResolveBaseUrl(IConfiguration configuration)
    {
        var url = configuration["QrMenu:BaseUrl"]?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(url) && !IsLocalhostBase(url))
            return url;

        url = Environment.GetEnvironmentVariable("QrMenu__BaseUrl")?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(url) && !IsLocalhostBase(url))
            return url;

        if (IsDevelopmentEnvironment(configuration))
            return "http://localhost:5173/r";

        return "http://localhost:5173/r";
    }
}
