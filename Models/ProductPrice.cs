namespace Vkeram.Backend.Models;

public class ProductPrice
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
