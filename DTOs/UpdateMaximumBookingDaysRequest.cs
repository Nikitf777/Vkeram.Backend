namespace Vkeram.Backend.DTOs;

public class UpdateMaximumBookingDaysRequest
{
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
