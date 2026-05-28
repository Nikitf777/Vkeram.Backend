using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class WorkDay
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string DayName { get; set; } = string.Empty;

    public bool IsWorkingDay { get; set; }
}
