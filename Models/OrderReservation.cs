using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class OrderReservation
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    public DateOnly Day { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public List<ProductReservation> ProductReservations { get; set; } = new();
}
