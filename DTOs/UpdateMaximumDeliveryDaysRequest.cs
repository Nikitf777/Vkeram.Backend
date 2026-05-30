namespace Vkeram.Backend.DTOs;

public class UpdateMaximumDeliveryDaysRequest
{
    public int Days { get; set; }
    public bool CountWorkingDaysOnly { get; set; }
}
