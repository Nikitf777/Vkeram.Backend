namespace Vkeram.Backend.DTOs;

public class AllowBookingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AllowBookingInfo? AllowBooking { get; set; }
}

public class AllowBookingInfo
{
    public int Id { get; set; }
    public bool IsAllowed { get; set; }
}
