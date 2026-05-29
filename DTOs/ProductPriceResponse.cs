namespace Vkeram.Backend.DTOs;

public class ProductPriceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ProductPriceInfo? ProductPrice { get; set; }
}

public class ProductPriceInfo
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
