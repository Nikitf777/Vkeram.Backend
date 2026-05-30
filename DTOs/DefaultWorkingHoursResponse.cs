namespace Vkeram.Backend.DTOs;

public class DefaultWorkingHoursResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DefaultWorkingHoursInfo? WorkingHours { get; set; }
}

public class DefaultWorkingHoursInfo
{
    public int Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
