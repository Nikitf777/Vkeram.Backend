namespace Vkeram.Backend.DTOs;

public class AddProductPriceRequest
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
