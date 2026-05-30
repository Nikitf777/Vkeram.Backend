using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class OrderDelivery
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    public DateTime DeliveryTime { get; set; }

    public bool Delivered { get; set; }

    public List<ProductReservation> ProductReservations { get; set; } = new();
}
