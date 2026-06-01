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

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int ProductPriceId { get; set; }

    [ForeignKey(nameof(ProductPriceId))]
    public ProductPrice ProductPrice { get; set; } = null!;

    public decimal Vat { get; set; }
}
