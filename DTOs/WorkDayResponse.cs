namespace Vkeram.Backend.DTOs;

public class WorkDayResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<WorkDayInfo>? WorkDays { get; set; }
    public WorkDayInfo? WorkDay { get; set; }
}

public class WorkDayInfo
{
    public int Id { get; set; }
    public string DayName { get; set; } = string.Empty;
    public bool IsWorkingDay { get; set; }
}
