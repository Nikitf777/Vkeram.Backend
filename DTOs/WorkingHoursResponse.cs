namespace Vkeram.Backend.DTOs;

public class WorkingHoursResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public WorkingHoursInfo? WorkingHours { get; set; }
}

public class WorkingHoursInfo
{
    public int Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
