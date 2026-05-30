namespace Vkeram.Backend.DTOs;

public class MaximumBookingDaysResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MaximumBookingDaysInfo? MaximumBookingDays { get; set; }
}

public class MaximumBookingDaysInfo
{
    public int Id { get; set; }
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
