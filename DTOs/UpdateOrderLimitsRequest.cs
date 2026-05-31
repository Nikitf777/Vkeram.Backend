namespace Vkeram.Backend.DTOs;

public class UpdateOrderLimitsRequest
{
    public decimal MinOrderPrice { get; set; }
    public decimal MaxOrderPrice { get; set; }
    public int MinOrderQuantity { get; set; }
    public int MaxOrderQuantity { get; set; }
    public int MinReservationQuantity { get; set; }
    public int MaxReservationQuantity { get; set; }
    public int MinDeliveryQuantity { get; set; }
    public int MaxDeliveryQuantity { get; set; }
    public int MinProductReservationQuantity { get; set; }
    public int MaxProductReservationQuantity { get; set; }
}
