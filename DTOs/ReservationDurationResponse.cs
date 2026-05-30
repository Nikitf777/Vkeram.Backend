namespace Vkeram.Backend.DTOs;

public class ReservationDurationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ReservationDurationInfo? ReservationDuration { get; set; }
}

public class ReservationDurationInfo
{
    public int Id { get; set; }
    public int DurationMinutes { get; set; }
}
