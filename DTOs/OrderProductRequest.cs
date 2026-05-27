using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class OrderProductRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
