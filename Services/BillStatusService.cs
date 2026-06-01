using System.Net.Http.Json;

namespace Vkeram.Backend.Services;

public class BillStatusService : IBillStatusService
{
    private readonly HttpClient _httpClient;
    private readonly string _billsPath;

    public BillStatusService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _billsPath = config["OneC:BillsPath"] ?? "/demo/hs/bills/create";
    }

    public async Task<BillStatusResponse?> GetBillStatusAsync(string billId)
    {
        var response = await _httpClient.GetAsync($"/demo/hs/bills/{billId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BillStatusResponse>();
    }

    public async Task UpdateBillStatusAsync(UpdateBillStatusRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync("/demo/hs/bills/status", request);
        response.EnsureSuccessStatusCode();
    }
}
