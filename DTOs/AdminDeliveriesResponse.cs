namespace Vkeram.Backend.DTOs;

public class AdminDeliveriesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AdminDeliveryItem> Deliveries { get; set; } = [];
}

public class AdminDeliveryItem
{
    public int Id { get; set; }
    public DateTime DeliveryTime { get; set; }
    public bool Delivered { get; set; }
    public int OrderId { get; set; }
    public bool IsConfirmed { get; set; }
}
