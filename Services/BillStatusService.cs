using System.Net.Http.Json;

namespace Vkeram.Backend.Services;

public class BillStatusService : IBillStatusService
{
    private readonly HttpClient _httpClient;

    public BillStatusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
