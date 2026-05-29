namespace Vkeram.Backend.DTOs;

public class ProductWithPriceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
}
