using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class ReservationDuration
{
    public int Id { get; set; }

    [Range(1, 480)]
    public int DurationMinutes { get; set; } = 30;
}
