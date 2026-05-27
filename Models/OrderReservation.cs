using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class OrderReservation
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}
