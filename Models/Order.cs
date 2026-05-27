using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Status { get; set; } = OrderStatus.PENDING_PAYMENT.ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderReservation> Reservations { get; set; } = new();
}
