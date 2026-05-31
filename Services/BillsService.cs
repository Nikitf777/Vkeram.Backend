using System.Net.Http.Json;

namespace Vkeram.Backend.Services;

public class BillsService : IBillsService
{
    private readonly HttpClient _httpClient;
    private readonly string _billsPath;

    public BillsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _billsPath = config["OneC:BillsPath"] ?? "/demo/hs/bills/create";
    }

    public async Task<string?> CreateBillAsync(CreateBillRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(_billsPath, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateBillResponse>();
        return result?.Id;
    }
}
