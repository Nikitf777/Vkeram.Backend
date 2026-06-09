namespace Vkeram.Backend.Services;

public interface IOrderConfirmationService
{
    Task<string?> ConfirmOrderAsync(Models.Order order);
}

public class OrderConfirmationService : IOrderConfirmationService
{
    private readonly IBillsService _billsService;
    private readonly IBillStatusService _billStatusService;

    public OrderConfirmationService(IBillsService billsService, IBillStatusService billStatusService)
    {
        _billsService = billsService;
        _billStatusService = billStatusService;
    }

    public async Task<string?> ConfirmOrderAsync(Models.Order order)
    {
        var billProducts = order.Reservations
            .SelectMany(r => r.ProductReservations)
            .Concat(order.Deliveries.SelectMany(d => d.ProductReservations))
            .GroupBy(pr => pr.ProductId)
            .Select(g => new BillProductDto
            {
                Id = g.Key,
                Quantity = g.Sum(pr => pr.Quantity),
                Price = g.First().ProductPrice?.Price ?? 0,
                DiscountPercent = 0
            })
            .ToList();

        var billRequest = new CreateBillRequest
        {
            BuyerId = order.User.BuyerId,
            Products = billProducts
        };

        var billId = await _billsService.CreateBillAsync(billRequest);
        if (billId == null) return null;

        order.IsConfirmed = true;
        order.BillId = billId;

        var shipmentStatus = ComputeShipmentStatus(order.Reservations, order.Deliveries);
        await _billStatusService.UpdateBillStatusAsync(new UpdateBillStatusRequest
        {
            BillId = billId,
            PaymentStatus = NormalizeStatusForApi(Models.PaymentStatus.Unpaid.ToString()),
            ShipmentStatus = NormalizeStatusForApi(shipmentStatus)
        });

        return billId;
    }

    public static string ComputeShipmentStatus(List<Models.OrderReservation> reservations, List<Models.OrderDelivery> deliveries)
    {
        if (reservations.Count == 0 && deliveries.Count == 0)
            return Models.ShipmentStatus.Unshipped.ToString();

        var allPicked = reservations.Count == 0 || reservations.All(r => r.Picked);
        var allDelivered = deliveries.Count == 0 || deliveries.All(d => d.Delivered);

        if (allPicked && allDelivered)
            return Models.ShipmentStatus.Shipped.ToString();

        if (reservations.Any(r => r.Picked) || deliveries.Any(d => d.Delivered))
            return Models.ShipmentStatus.PartiallyShipped.ToString();

        return Models.ShipmentStatus.Unshipped.ToString();
    }

    public static string NormalizeStatusForApi(string status)
    {
        return status switch
        {
            "Shipped" => "shipped",
            "PartiallyShipped" => "partial",
            "Unshipped" => "unshipped",
            "Paid" => "paid",
            "PartiallyPaid" => "partial",
            "Unpaid" => "unpaid",
            _ => status.ToLowerInvariant()
        };
    }
}
