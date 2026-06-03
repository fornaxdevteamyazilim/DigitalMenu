using System.Text.Json;

namespace DigitalMenu.Core.Models;

public static class ProductOptionsJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static string Serialize(ProductOptions options) =>
        JsonSerializer.Serialize(options, SerializerOptions);

    public static ProductOptions Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new ProductOptions();

        try
        {
            return JsonSerializer.Deserialize<ProductOptions>(json, SerializerOptions) ?? new ProductOptions();
        }
        catch
        {
            return new ProductOptions();
        }
    }
}
