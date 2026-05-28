using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class ProductReservation
{
    public int Id { get; set; }

    public int? OrderReservationId { get; set; }

    [ForeignKey(nameof(OrderReservationId))]
    public OrderReservation? OrderReservation { get; set; }

    public int? OrderDeliveryId { get; set; }

    [ForeignKey(nameof(OrderDeliveryId))]
    public OrderDelivery? OrderDelivery { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
