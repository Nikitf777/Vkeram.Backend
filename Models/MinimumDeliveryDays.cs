using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class MinimumDeliveryDays
{
    public int Id { get; set; }

    [Range(1, 365)]
    public int Days { get; set; }
}
