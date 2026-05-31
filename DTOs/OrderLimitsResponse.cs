namespace Vkeram.Backend.DTOs;

public class OrderLimitsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderLimitsInfo? OrderLimits { get; set; }
}

public class OrderLimitsInfo
{
    public int Id { get; set; }
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
