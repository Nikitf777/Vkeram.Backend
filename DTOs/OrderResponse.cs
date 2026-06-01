namespace Vkeram.Backend.DTOs;

public class OrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public bool IsConfirmed { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ShipmentStatus { get; set; }
    public int? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ReservationInfo>? Reservations { get; set; }
    public List<DeliveryInfo>? Deliveries { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
}

public class AdminOrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public bool IsConfirmed { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ShipmentStatus { get; set; }
    public int? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ReservationInfo
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Picked { get; set; }
    public List<ProductReservationInfo>? Products { get; set; }
}

public class ProductReservationInfo
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}

public class DeliveryInfo
{
    public int Id { get; set; }
    public DateTime DeliveryTime { get; set; }
    public bool Delivered { get; set; }
    public List<ProductReservationInfo>? Products { get; set; }
}

public class ReservationSlotInfo
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
