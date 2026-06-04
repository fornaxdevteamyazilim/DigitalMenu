using Microsoft.Extensions.Configuration;

namespace DigitalMenu.Infrastructure.Services;

public static class QrMenuUrlBuilder
{
    /// <summary>Railway QR Menü — appsettings / env yoksa production fallback.</summary>
    public const string ProductionDefaultBaseUrl = "https://triumphant-abundance-production-1cb2.up.railway.app/r";

    public static string Build(string baseUrl, string tenantId, string tableNumber)
    {
        return $"{baseUrl.TrimEnd('/')}/{tenantId}?table={Uri.EscapeDataString(tableNumber)}";
    }

    public static bool IsLocalhostBase(string? baseUrl) =>
        string.IsNullOrWhiteSpace(baseUrl)
        || baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
        || baseUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

    /// <summary>Veritabanında seed/deploy sırasında kalan localhost QR linkleri.</summary>
    public static bool IsStaleQrCodeUrl(string? qrCodeUrl) =>
        string.IsNullOrWhiteSpace(qrCodeUrl)
        || qrCodeUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
        || qrCodeUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldUpdateQrCodeUrl(string? currentUrl, string newUrl, bool forceAllMismatches) =>
        !string.Equals(currentUrl, newUrl, StringComparison.Ordinal)
        && (forceAllMismatches || IsStaleQrCodeUrl(currentUrl));

    /// <summary>Railway/Docker: PORT veya Railway meta — Development env yanlış set olsa bile localhost kullanılmaz.</summary>
    public static bool IsDeployedEnvironment() =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"))
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID"))
        || (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT"))
            && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DATABASE_URL")));

    public static bool IsDevelopmentEnvironment(IConfiguration configuration) =>
        !IsDeployedEnvironment()
        && (string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase));

    public static string ResolveBaseUrl(IConfiguration configuration)
    {
        foreach (var candidate in GetCandidates(configuration))
        {
            if (!string.IsNullOrWhiteSpace(candidate) && !IsLocalhostBase(candidate))
                return candidate.TrimEnd('/');
        }

        if (IsDevelopmentEnvironment(configuration))
            return "http://localhost:5173/r";

        return ProductionDefaultBaseUrl.TrimEnd('/');
    }

    private static IEnumerable<string?> GetCandidates(IConfiguration configuration)
    {
        yield return configuration["QrMenu:BaseUrl"];
        yield return Environment.GetEnvironmentVariable("QrMenu__BaseUrl");
        yield return Environment.GetEnvironmentVariable("QRMENU__BASEURL");
    }
}
