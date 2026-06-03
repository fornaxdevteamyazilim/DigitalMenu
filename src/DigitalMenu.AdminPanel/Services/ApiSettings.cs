namespace DigitalMenu.AdminPanel.Services;

public sealed class ApiSettings
{
    public string BaseUrl { get; init; } = "https://localhost:7182";
}

internal sealed class ClientAppSettings
{
    public string? ApiBaseUrl { get; set; }
}
