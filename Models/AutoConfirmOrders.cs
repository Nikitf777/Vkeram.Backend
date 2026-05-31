using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class AutoConfirmOrders
{
    public int Id { get; set; }

    public bool IsEnabled { get; set; } = false;

    public decimal MaxAutoConfirmPrice { get; set; } = 10000;

    public int MaxAutoConfirmQuantity { get; set; } = 100;
}
