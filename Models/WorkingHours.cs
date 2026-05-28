namespace Vkeram.Backend.Models;

public class WorkingHours
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
