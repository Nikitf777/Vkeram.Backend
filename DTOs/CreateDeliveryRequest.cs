using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class CreateDeliveryRequest
{
    [Required]
    public DateTime DeliveryTime { get; set; }

    [Required, MinLength(1)]
    public List<OrderProductRequest> Products { get; set; } = new();
}
