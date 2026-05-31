using System.Net.Http.Json;
using System.Text.Json;

namespace Vkeram.Backend.Services;

public class BuyersService : IBuyersService
{
    private readonly HttpClient _httpClient;
    private readonly string _buyersPath;
    private readonly string _buyerDetailPath;

    public BuyersService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _buyersPath = config["OneC:BuyersPath"] ?? "/demo/hs/buyers/all";
        _buyerDetailPath = config["OneC:BuyerDetailPath"] ?? "/demo/hs/buyers/";
    }

    public async Task<List<BuyerDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync(_buyersPath);
        response.EnsureSuccessStatusCode();

        var rawBuyers = await response.Content.ReadFromJsonAsync<List<BuyerDto>>();
        return rawBuyers ?? new List<BuyerDto>();
    }

    public async Task<JsonElement?> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync(_buyerDetailPath + Uri.EscapeDataString(id));
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content).RootElement.Clone();
    }
}
