namespace Vkeram.Backend.Models;

public record OrderProductLine(DateTime CreatedAt, string ProductId, int Quantity, decimal UnitPrice, decimal Vat, string BuyerId);
