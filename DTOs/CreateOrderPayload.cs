namespace Vkeram.Backend.DTOs;

public class CreateOrderPayload
{
    public List<CreateOrderRequest> Reservations { get; set; } = new();
    public List<CreateDeliveryRequest> Deliveries { get; set; } = new();
}
