namespace Vkeram.Backend.DTOs;

public class AllowDeliveryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AllowDeliveryInfo? AllowDelivery { get; set; }
}

public class AllowDeliveryInfo
{
    public int Id { get; set; }
    public bool IsAllowed { get; set; }
}
