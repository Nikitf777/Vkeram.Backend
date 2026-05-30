namespace Vkeram.Backend.DTOs;

public class MaximumDeliveryDaysResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MaximumDeliveryDaysInfo? MaximumDeliveryDays { get; set; }
}

public class MaximumDeliveryDaysInfo
{
    public int Id { get; set; }
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
