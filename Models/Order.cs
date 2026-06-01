using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vkeram.Backend.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public bool IsConfirmed { get; set; } = false;

    [MaxLength(200)]
    public string? BillId { get; set; }

    [NotMapped]
    public string PaymentStatus { get; set; } = Models.PaymentStatus.Unpaid.ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderReservation> Reservations { get; set; } = new();

    public List<OrderDelivery> Deliveries { get; set; } = new();

    [NotMapped]
    public string ShipmentStatus { get; set; } = Models.ShipmentStatus.Unshipped.ToString();

    [NotMapped]
    public decimal TotalPrice =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity * (1 + pr.Vat / 100m)))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => (pr.ProductPrice?.Price ?? 0) * pr.Quantity * (1 + pr.Vat / 100m)));

    [NotMapped]
    public int TotalQuantity =>
        Reservations.Sum(r => r.ProductReservations.Sum(pr => pr.Quantity))
        + Deliveries.Sum(d => d.ProductReservations.Sum(pr => pr.Quantity));
}
