namespace Vkeram.Backend.DTOs;

public class MinimumBookingDaysResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MinimumBookingDaysInfo? MinimumBookingDays { get; set; }
}

public class MinimumBookingDaysInfo
{
    public int Id { get; set; }
    public int Days { get; set; }
}
