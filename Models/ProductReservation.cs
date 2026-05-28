namespace Vkeram.Backend.Models;

public class ProductReservation
{
    public int Id { get; set; }

    public int OrderReservationId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
