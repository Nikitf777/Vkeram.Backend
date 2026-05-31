namespace Vkeram.Backend.Services;

public class BillProductDto
{
    public string Id { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int DiscountPercent { get; set; } = 0;
}

public class CreateBillRequest
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BillProductDto> Products { get; set; } = new();
}

public class CreateBillResponse
{
    public string Id { get; set; } = string.Empty;
}

public interface IBillsService
{
    Task<string?> CreateBillAsync(CreateBillRequest request);
}
