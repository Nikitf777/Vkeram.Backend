using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class ProductReservation
{
    public int Id { get; set; }

    public int OrderReservationId { get; set; }

    [ForeignKey(nameof(OrderReservationId))]
    public OrderReservation OrderReservation { get; set; } = null!;

    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
