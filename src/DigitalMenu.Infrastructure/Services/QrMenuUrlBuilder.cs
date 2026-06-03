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
}
