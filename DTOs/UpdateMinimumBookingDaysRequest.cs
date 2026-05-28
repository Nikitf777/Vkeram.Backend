namespace Vkeram.Backend.DTOs;

public class UpdateMinimumBookingDaysRequest
{
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
