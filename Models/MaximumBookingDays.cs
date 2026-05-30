using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class MaximumBookingDays
{
    public int Id { get; set; }

    [Range(1, 365)]
    public int Days { get; set; }

    public bool CountWorkingDaysOnly { get; set; }
}
