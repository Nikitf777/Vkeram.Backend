using System.Net.Http.Json;

namespace Vkeram.Backend.Services;

internal class RawProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, ProductDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();

        var rawProducts = await response.Content.ReadFromJsonAsync<List<RawProductDto>>();

        if (rawProducts == null)
            return new Dictionary<string, ProductDto>();

        var idSet = ids.ToHashSet();
        var result = new Dictionary<string, ProductDto>();

        foreach (var raw in rawProducts)
        {
            if (idSet.Contains(raw.Id))
            {
                result[raw.Id] = new ProductDto { Id = raw.Id, Name = raw.Name };
            }
        }

        return result;
    }
}
