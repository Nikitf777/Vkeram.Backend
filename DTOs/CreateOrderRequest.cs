using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class CreateOrderRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required, MinLength(1)]
    public List<OrderProductRequest> Products { get; set; } = new();
}
