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
    public string ConfirmationStatus { get; set; } = Models.ConfirmationStatus.Confirmed.ToString();

    [Required, MaxLength(50)]
    public string PaymentStatus { get; set; } = Models.PaymentStatus.Unpaid.ToString();

    [Required, MaxLength(50)]
    public string ShipmentStatus { get; set; } = Models.ShipmentStatus.Unshipped.ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderReservation> Reservations { get; set; } = new();

    public List<OrderDelivery> Deliveries { get; set; } = new();

    [NotMapped]
    public decimal TotalPrice =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity));

    [NotMapped]
    public int TotalQuantity =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => pr.Quantity))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => pr.Quantity));
}
