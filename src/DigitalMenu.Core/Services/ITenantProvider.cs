namespace DigitalMenu.Core.Services;

public interface ITenantProvider
{
    string? GetTenantId();
}
