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

    [MaxLength(200)]
    public string? BillId { get; set; }

    [Required, MaxLength(50)]
    public string PaymentStatus { get; set; } = Models.PaymentStatus.Unpaid.ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderReservation> Reservations { get; set; } = new();

    public List<OrderDelivery> Deliveries { get; set; } = new();

    [NotMapped]
    public string ShipmentStatus
    {
        get
        {
            if (Reservations.Count == 0 && Deliveries.Count == 0)
                return Models.ShipmentStatus.Unshipped.ToString();

            var allPicked = Reservations.Count == 0 || Reservations.All(r => r.Picked);
            var allDelivered = Deliveries.Count == 0 || Deliveries.All(d => d.Delivered);

            if (allPicked && allDelivered)
                return Models.ShipmentStatus.Shipped.ToString();

            if (Reservations.Any(r => r.Picked) || Deliveries.Any(d => d.Delivered))
                return Models.ShipmentStatus.PartiallyShipped.ToString();

            return Models.ShipmentStatus.Unshipped.ToString();
        }
    }

    [NotMapped]
    public decimal TotalPrice =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity));

    [NotMapped]
    public int TotalQuantity =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => pr.Quantity))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => pr.Quantity));
}
