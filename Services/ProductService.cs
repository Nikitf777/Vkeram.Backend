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

    public async Task<Dictionary<int, ProductDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var response = await _httpClient.GetAsync("");
        response.EnsureSuccessStatusCode();

        var rawProducts = await response.Content.ReadFromJsonAsync<List<RawProductDto>>();

        if (rawProducts == null)
            return new Dictionary<int, ProductDto>();

        var idSet = ids.ToHashSet();
        var result = new Dictionary<int, ProductDto>();

        foreach (var raw in rawProducts)
        {
            if (int.TryParse(raw.Id, out var id) && idSet.Contains(id))
            {
                result[id] = new ProductDto { Id = id, Name = raw.Name };
            }
        }

        return result;
    }
}
