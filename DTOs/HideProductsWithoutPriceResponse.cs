namespace Vkeram.Backend.DTOs;

public class HideProductsWithoutPriceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public HideProductsWithoutPriceInfo? HideProductsWithoutPrice { get; set; }
}

public class HideProductsWithoutPriceInfo
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
}
