using System.Net.Http.Json;

namespace Vkeram.Backend.Services;

internal class RawProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Vat { get; set; }
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly string _productsPath;

    public ProductService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _productsPath = config["OneC:ProductsPath"] ?? "/demo/hs/products";
    }

    public async Task<Dictionary<string, ProductDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var tasks = ids.Select(async id =>
        {
            try
            {
                var product = await GetByIdAsync(id);
                return product != null ? new KeyValuePair<string, ProductDto>(id, product) : default;
            }
            catch
            {
                return default;
            }
        });

        var results = await Task.WhenAll(tasks);
        var result = new Dictionary<string, ProductDto>();

        foreach (var kvp in results)
        {
            if (kvp.Key != null)
                result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync($"{_productsPath}/all");
        response.EnsureSuccessStatusCode();

        var rawProducts = await response.Content.ReadFromJsonAsync<List<RawProductDto>>();

        if (rawProducts == null)
            return new List<ProductDto>();

        return rawProducts.Select(r => MapToDto(r)).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"{_productsPath}/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();

        var rawProduct = await response.Content.ReadFromJsonAsync<RawProductDto>();

        if (rawProduct == null)
            return null;

        return MapToDto(rawProduct);
    }

    private static ProductDto MapToDto(RawProductDto raw)
    {
        var vat = 0m;
        if (!string.IsNullOrWhiteSpace(raw.Vat))
        {
            var cleaned = raw.Vat.TrimEnd('%', ' ');
            decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out vat);
        }

        return new ProductDto
        {
            Id = raw.Id,
            Name = raw.Name,
            Vat = vat
        };
    }
}
