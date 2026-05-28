namespace Vkeram.Backend.DTOs;

public class OrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? ConfirmationStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ShipmentStatus { get; set; }
    public int? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ReservationInfo>? Reservations { get; set; }
}

public class AdminOrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? ConfirmationStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ShipmentStatus { get; set; }
    public int? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ReservationInfo
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ProductReservationInfo>? Products { get; set; }
}

public class ProductReservationInfo
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ReservationSlotInfo
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
