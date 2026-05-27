using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public List<ProductReservation> ProductReservations { get; set; } = new();
}
