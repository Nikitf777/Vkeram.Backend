namespace Vkeram.Backend.DTOs;

public class UpdateAutoConfirmOrdersRequest
{
    public bool IsEnabled { get; set; }
    public decimal MaxAutoConfirmPrice { get; set; }
    public int MaxAutoConfirmQuantity { get; set; }
}
