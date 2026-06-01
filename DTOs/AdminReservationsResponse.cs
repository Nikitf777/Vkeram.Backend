namespace Vkeram.Backend.DTOs;

public class AdminReservationsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AdminReservationItem> Reservations { get; set; } = [];
}

public class AdminReservationItem
{
    public int Id { get; set; }
    public DateOnly Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool Picked { get; set; }
    public int OrderId { get; set; }
    public bool IsConfirmed { get; set; }
}
