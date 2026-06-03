using System.Net.Http.Json;

namespace DigitalMenu.AdminPanel.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    // Test amaçlı şimdilik seed data ile eklediğimiz restoranın ID'sini sabitliyoruz.
    // İleride burası login olan kullanıcının tenantId'si olacak.
    private readonly string _currentTenantId = "lezzet-duragi";

    public string CurrentTenantId => _currentTenantId;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Tüm isteklere otomatik olarak hangi restoran olduğumuzu ekliyoruz
        if (!_httpClient.DefaultRequestHeaders.Contains("X-Tenant-Id"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", _currentTenantId);
        }
    }

    public async Task<List<T>?> GetAsync<T>(string url)
    {
        return await _httpClient.GetFromJsonAsync<List<T>>(url);
    }

    public async Task<T?> GetSingleAsync<T>(string url)
    {
        return await _httpClient.GetFromJsonAsync<T>(url);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        return await _httpClient.PostAsJsonAsync(url, data);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        return await _httpClient.PutAsJsonAsync(url, data);
    }

    public async Task<HttpResponseMessage> PostEmptyAsync(string url)
    {
        return await _httpClient.PostAsync(url, null);
    }
}
