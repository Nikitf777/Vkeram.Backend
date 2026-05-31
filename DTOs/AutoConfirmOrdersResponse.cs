namespace Vkeram.Backend.DTOs;

public class AutoConfirmOrdersResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AutoConfirmOrdersInfo? AutoConfirmOrders { get; set; }
}

public class AutoConfirmOrdersInfo
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public decimal MaxAutoConfirmPrice { get; set; }
    public int MaxAutoConfirmQuantity { get; set; }
}
