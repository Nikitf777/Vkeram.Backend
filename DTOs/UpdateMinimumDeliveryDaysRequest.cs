namespace Vkeram.Backend.DTOs;

public class UpdateMinimumDeliveryDaysRequest
{
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
