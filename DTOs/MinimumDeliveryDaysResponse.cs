namespace Vkeram.Backend.DTOs;

public class MinimumDeliveryDaysResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MinimumDeliveryDaysInfo? MinimumDeliveryDays { get; set; }
}

public class MinimumDeliveryDaysInfo
{
    public int Id { get; set; }
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
