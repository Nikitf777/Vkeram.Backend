namespace Vkeram.Backend.Services;

public class BillStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public string Buyer { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string ShipmentStatus { get; set; } = string.Empty;
}

public class UpdateBillStatusRequest
{
    public string BillId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string ShipmentStatus { get; set; } = string.Empty;
}

public interface IBillStatusService
{
    Task<BillStatusResponse?> GetBillStatusAsync(string billId);
    Task UpdateBillStatusAsync(UpdateBillStatusRequest request);
}
