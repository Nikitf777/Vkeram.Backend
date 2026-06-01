namespace Vkeram.Backend.DTOs;

public class ProductWithPriceAndCharacteristicsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal Vat { get; set; }
    public ProductCharacteristicDto? Characteristics { get; set; }
    public string? PreviewUrl { get; set; }
}
