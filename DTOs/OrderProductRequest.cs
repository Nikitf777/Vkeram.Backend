using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class OrderProductRequest
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
