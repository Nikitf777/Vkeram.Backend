using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class OrderLimits
{
    public int Id { get; set; }

    public decimal MinOrderPrice { get; set; } = 0;

    public decimal MaxOrderPrice { get; set; } = 1000000;

    public int MinOrderQuantity { get; set; } = 1;

    public int MaxOrderQuantity { get; set; } = 10000;

    public int MinReservationQuantity { get; set; } = 1;

    public int MaxReservationQuantity { get; set; } = 100;

    public int MinDeliveryQuantity { get; set; } = 1;

    public int MaxDeliveryQuantity { get; set; } = 100;

    public int MinProductReservationQuantity { get; set; } = 1;

    public int MaxProductReservationQuantity { get; set; } = 1000;
}
