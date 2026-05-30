namespace Vkeram.Backend.DTOs;

public class DefaultBreakResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DefaultBreakInfo? Break { get; set; }
}

public class DefaultBreakListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<DefaultBreakInfo> Breaks { get; set; } = new();
}

public class DefaultBreakInfo
{
    public int Id { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
