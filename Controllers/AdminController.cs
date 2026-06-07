using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IInviteCodeRepository _inviteRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IWorkDayRepository _workDayRepo;
    private readonly IDefaultWorkingHoursRepository _workingHoursRepo;
    private readonly IDefaultBreakRepository _breakRepo;
    private readonly IMinimumBookingDaysRepository _minBookingDaysRepo;
    private readonly IMaximumBookingDaysRepository _maxBookingDaysRepo;
    private readonly IMinimumDeliveryDaysRepository _minDeliveryDaysRepo;
    private readonly IMaximumDeliveryDaysRepository _maxDeliveryDaysRepo;
    private readonly IAllowBookingRepository _allowBookingRepo;
    private readonly IAllowDeliveryRepository _allowDeliveryRepo;
    private readonly IReservationDurationRepository _reservationDurationRepo;
    private readonly IOrderLimitsRepository _orderLimitsRepo;
    private readonly IAutoConfirmOrdersRepository _autoConfirmOrdersRepo;
    private readonly IBillsService _billsService;
    private readonly IProductPriceRepository _productPriceRepo;
    private readonly IProductImageRepository _productImageRepo;
    private readonly IProductCharacteristicRepository _productCharacteristicRepo;
    private readonly IProductImagePreviewRepository _productImagePreviewRepo;
    private readonly IImagePreviewService _imagePreviewService;
    private readonly IProductService _productService;
    private readonly IUserRepository _userRepo;
    private readonly IBillStatusService _billStatusService;
    private readonly IProductHiddenRepository _productHiddenRepo;
    private readonly IHideProductsWithoutPriceRepository _hideProductsWithoutPriceRepo;
    private readonly IBuyersService _buyersService;
    private readonly string _adminKey;

    public AdminController(IInviteCodeRepository inviteRepo, IOrderRepository orderRepo, IWorkDayRepository workDayRepo, IDefaultWorkingHoursRepository workingHoursRepo, IDefaultBreakRepository breakRepo, IMinimumBookingDaysRepository minBookingDaysRepo, IMaximumBookingDaysRepository maxBookingDaysRepo, IMinimumDeliveryDaysRepository minDeliveryDaysRepo, IMaximumDeliveryDaysRepository maxDeliveryDaysRepo, IAllowBookingRepository allowBookingRepo, IAllowDeliveryRepository allowDeliveryRepo, IReservationDurationRepository reservationDurationRepo, IOrderLimitsRepository orderLimitsRepo, IAutoConfirmOrdersRepository autoConfirmOrdersRepo, IBillsService billsService, IProductPriceRepository productPriceRepo, IProductImageRepository productImageRepo, IProductCharacteristicRepository productCharacteristicRepo, IProductImagePreviewRepository productImagePreviewRepo, IImagePreviewService imagePreviewService, IProductService productService, IUserRepository userRepo, IBillStatusService billStatusService, IProductHiddenRepository productHiddenRepo, IHideProductsWithoutPriceRepository hideProductsWithoutPriceRepo, IBuyersService buyersService, IConfiguration config)
    {
        _inviteRepo = inviteRepo;
        _orderRepo = orderRepo;
        _workDayRepo = workDayRepo;
        _workingHoursRepo = workingHoursRepo;
        _breakRepo = breakRepo;
        _minBookingDaysRepo = minBookingDaysRepo;
        _maxBookingDaysRepo = maxBookingDaysRepo;
        _minDeliveryDaysRepo = minDeliveryDaysRepo;
        _maxDeliveryDaysRepo = maxDeliveryDaysRepo;
        _allowBookingRepo = allowBookingRepo;
        _allowDeliveryRepo = allowDeliveryRepo;
        _reservationDurationRepo = reservationDurationRepo;
        _orderLimitsRepo = orderLimitsRepo;
        _autoConfirmOrdersRepo = autoConfirmOrdersRepo;
        _billsService = billsService;
        _productPriceRepo = productPriceRepo;
        _productImageRepo = productImageRepo;
        _productCharacteristicRepo = productCharacteristicRepo;
        _productImagePreviewRepo = productImagePreviewRepo;
        _imagePreviewService = imagePreviewService;
        _productService = productService;
        _userRepo = userRepo;
        _billStatusService = billStatusService;
        _productHiddenRepo = productHiddenRepo;
        _hideProductsWithoutPriceRepo = hideProductsWithoutPriceRepo;
        _buyersService = buyersService;
        _adminKey = config["AdminApiKey"] ?? "";
    }

    [HttpPost("invites")]
    public async Task<ActionResult<InviteResponse>> CreateInvites(
        [FromBody] CreateInviteRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (adminKey != _adminKey)
        {
            return Unauthorized(new InviteResponse
            {
                Success = false,
                Message = "Invalid admin key."
            });
        }

        if (request.Count < 1 || request.Count > 100)
        {
            return BadRequest(new InviteResponse
            {
                Success = false,
                Message = "Count must be between 1 and 100."
            });
        }

        var expiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays);
        var codes = new List<string>();
        var invites = new List<InviteCode>();

        for (int i = 0; i < request.Count; i++)
        {
            var code = $"B2B-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            invites.Add(new InviteCode
            {
                Code = code,
                 BuyerId = request.BuyerId,
                ExpiresAt = expiresAt
            });
            codes.Add(code);
        }

        await _inviteRepo.CreateRangeAsync(invites);

        return Ok(new InviteResponse
        {
            Success = true,
            Message = $"Created {request.Count} invite code(s).",
            Codes = codes,
            ExpiresAt = expiresAt
        });
    }

    [HttpGet("invites")]
    public async Task<ActionResult> ListInvites(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (adminKey != _adminKey)
        {
            return Unauthorized(new { Success = false, Message = "Invalid admin key." });
        }

        var inviteList = await _inviteRepo.GetAllAsync();
        var allUsers = await _userRepo.GetAllAsync();
        var usersById = allUsers.ToDictionary(u => u.Id);
        var invites = inviteList.Select(i => new
        {
            i.Id,
            i.Code,
            i.BuyerId,
            i.IsUsed,
            i.IsRevoked,
            i.UsedByUserId,
            UsedByBuyerId = i.UsedByUserId.HasValue && usersById.TryGetValue(i.UsedByUserId.Value, out var u) ? u.BuyerId : null,
            i.CreatedAt,
            i.UsedAt,
            i.ExpiresAt
        }).ToList();

        return Ok(new { Success = true, Invites = invites });
    }

    [HttpPost("invites/revoke")]
    public async Task<ActionResult> RevokeInvites(
        [FromBody] RevokeInvitesRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (adminKey != _adminKey)
        {
            return Unauthorized(new { Success = false, Message = "Invalid admin key." });
        }

        if (request.Ids == null || request.Ids.Count == 0)
        {
            return BadRequest(new { Success = false, Message = "No IDs provided." });
        }

        await _inviteRepo.RevokeRangeAsync(request.Ids);

        return Ok(new { Success = true, Message = $"Revoked {request.Ids.Count} invite code(s)." });
    }

    [HttpGet("users")]
    public async Task<ActionResult> GetUsers(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var users = await _userRepo.GetAllAsync();
        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);

        var result = users.Select(u => new
        {
            u.Id,
            u.BuyerId,
            BuyerName = buyerMap.GetValueOrDefault(u.BuyerId, ""),
            u.ContactEmail,
            u.ContactName,
            u.Phone,
            u.CreatedAt,
            u.IsActive
        }).ToList();

        return Ok(new { Success = true, Users = result });
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult> GetUser(
        int id,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);

        return Ok(new
        {
            Success = true,
            User = new
            {
                user.Id,
                user.BuyerId,
                BuyerName = buyerMap.GetValueOrDefault(user.BuyerId, ""),
                user.ContactEmail,
                user.ContactName,
                user.Phone,
                user.CreatedAt,
                user.IsActive
            }
        });
    }

    [HttpGet("users/{userId}/orders")]
    public async Task<ActionResult> GetUserOrders(
        int userId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        var orders = await _orderRepo.GetByUserIdAsync(userId);
        var result = orders.Select(o => new
        {
            o.Id,
            o.IsConfirmed,
            o.PaymentStatus,
            o.ShipmentStatus,
            o.CreatedAt,
            ReservationsCount = o.Reservations.Count,
            DeliveriesCount = o.Deliveries.Count,
            TotalPrice = o.TotalPrice,
            TotalQuantity = o.TotalQuantity
        }).ToList();

        return Ok(new { Success = true, Orders = result });
    }

    [HttpGet("orders")]
    public async Task<ActionResult> GetOrders(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? isConfirmed,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? shipmentStatus,
        [FromQuery] string? buyerId,
        [FromQuery] int? userId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var orders = await _orderRepo.GetAllAsync(from, to, isConfirmed, paymentStatus, shipmentStatus, buyerId, userId);
        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);

        var result = orders.Select(o => new
        {
            o.Id,
            UserId = o.User.Id,
            UserBuyerId = o.User.BuyerId,
            UserBuyerName = buyerMap.GetValueOrDefault(o.User.BuyerId, o.User.BuyerId),
            UserContactName = o.User.ContactName,
            o.IsConfirmed,
            o.PaymentStatus,
            o.ShipmentStatus,
            o.CreatedAt,
            ReservationsCount = o.Reservations.Count,
            DeliveriesCount = o.Deliveries.Count,
            TotalPrice = o.TotalPrice,
            TotalQuantity = o.TotalQuantity
        }).ToList();

        return Ok(new { Success = true, Orders = result });
    }

    [HttpGet("orders/by-product/{productId}")]
    public async Task<ActionResult> GetOrdersByProduct(
        string productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? isConfirmed,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? shipmentStatus,
        [FromQuery] string? buyerId,
        [FromQuery] int? userId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var orders = await _orderRepo.GetByProductIdAsync(productId, from, to, isConfirmed, paymentStatus, shipmentStatus, buyerId, userId);
        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);

        var result = orders.Select(o => new
        {
            o.Id,
            UserId = o.User.Id,
            UserBuyerId = o.User.BuyerId,
            UserBuyerName = buyerMap.GetValueOrDefault(o.User.BuyerId, o.User.BuyerId),
            UserContactName = o.User.ContactName,
            o.IsConfirmed,
            o.PaymentStatus,
            o.ShipmentStatus,
            o.CreatedAt,
            ReservationsCount = o.Reservations.Count,
            DeliveriesCount = o.Deliveries.Count,
            TotalPrice = o.TotalPrice,
            TotalQuantity = o.TotalQuantity,
            ProductQuantity = o.Reservations.Sum(r => r.ProductReservations.Where(pr => pr.ProductId == productId).Sum(pr => pr.Quantity))
                         + o.Deliveries.Sum(d => d.ProductReservations.Where(pr => pr.ProductId == productId).Sum(pr => pr.Quantity))
        }).ToList();

        return Ok(new { Success = true, Orders = result });
    }

    [HttpGet("orders/{orderId}")]
    public async Task<ActionResult> GetOrderDetail(
        int orderId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
            return NotFound(new { Success = false, Message = "Order not found." });

        var allProductIds = order.Reservations
            .SelectMany(r => r.ProductReservations.Select(p => p.ProductId))
            .Concat(order.Deliveries.SelectMany(d => d.ProductReservations.Select(p => p.ProductId)))
            .Distinct()
            .ToList();

        var productMap = allProductIds.Count > 0
            ? await _productService.GetByIdsAsync(allProductIds)
            : new Dictionary<string, ProductDto>();

        var (paymentStatus, shipmentStatus) = await GetBillStatusOrDefault(order);

        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order retrieved.",
            OrderId = order.Id,
            IsConfirmed = order.IsConfirmed,
            PaymentStatus = paymentStatus,
            ShipmentStatus = shipmentStatus,
            UserId = order.UserId,
            UserBuyerId = order.User.BuyerId,
            UserBuyerName = buyerMap.GetValueOrDefault(order.User.BuyerId, order.User.BuyerId),
            CreatedAt = order.CreatedAt,
            Reservations = order.Reservations.Select(r => new ReservationInfo
            {
                Id = r.Id,
                StartTime = r.Day.ToDateTime(r.StartTime),
                EndTime = r.Day.ToDateTime(r.EndTime),
                Picked = r.Picked,
                Products = r.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                    Quantity = pr.Quantity,
                    Vat = pr.Vat,
                    Price = (pr.ProductPrice?.Price ?? 0) * (1 + pr.Vat / 100m),
                    TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity * (1 + pr.Vat / 100m)
                }).ToList()
            }).ToList(),
            Deliveries = order.Deliveries.Select(d => new DeliveryInfo
            {
                Id = d.Id,
                DeliveryTime = d.DeliveryTime,
                Delivered = d.Delivered,
                Products = d.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                    Quantity = pr.Quantity,
                    Vat = pr.Vat,
                    Price = (pr.ProductPrice?.Price ?? 0) * (1 + pr.Vat / 100m),
                    TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity * (1 + pr.Vat / 100m)
                }).ToList()
            }).ToList(),
            TotalPrice = order.TotalPrice,
            TotalQuantity = order.TotalQuantity
        });
    }

    private bool IsValidAdmin(string adminKey) => adminKey == _adminKey;

    private ActionResult UnauthorizedResponse() =>
        Unauthorized(new { Success = false, Message = "Invalid admin key." });

    private static readonly string[] ValidPaymentStatuses = ["Paid", "PartiallyPaid", "Unpaid"];

    private static string NormalizeStatusForApi(string status) =>
        string.IsNullOrEmpty(status) ? status : char.ToLower(status[0]) + status[1..];

    private static string NormalizeStatusFromApi(string status) =>
        string.IsNullOrEmpty(status) ? status : char.ToUpper(status[0]) + status[1..];

    private async Task<(string paymentStatus, string shipmentStatus)> GetBillStatusOrDefault(Order order)
    {
        if (order.BillId == null)
            return (Models.PaymentStatus.Unpaid.ToString(), ComputeShipmentStatus(order.Reservations, order.Deliveries));

        try
        {
            var billStatus = await _billStatusService.GetBillStatusAsync(order.BillId);
            if (billStatus != null)
                return (NormalizeStatusFromApi(billStatus.PaymentStatus), NormalizeStatusFromApi(billStatus.ShipmentStatus));
        }
        catch
        {
        }

        return (Models.PaymentStatus.Unpaid.ToString(), ComputeShipmentStatus(order.Reservations, order.Deliveries));
    }

    private static string ComputeShipmentStatus(ICollection<OrderReservation> reservations, ICollection<OrderDelivery> deliveries)
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

    [HttpPost("orders/{orderId}/confirm")]
    public async Task<ActionResult<AdminOrderResponse>> ConfirmOrder(
        int orderId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound(new AdminOrderResponse
            {
                Success = false,
                Message = "Order not found."
            });
        }

        if (order.IsConfirmed)
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = "Order is already confirmed."
            });
        }

        var user = await _userRepo.GetByIdAsync(order.UserId);
        if (user == null)
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = "User not found."
            });
        }

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
            BuyerId = user.BuyerId,
            Products = billProducts
        };

        var billId = await _billsService.CreateBillAsync(billRequest);
        if (billId == null)
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = "Failed to create bill."
            });
        }

        order.IsConfirmed = true;
        order.BillId = billId;
        await _orderRepo.UpdateAsync(order);

        var computedShipment = ComputeShipmentStatus(order.Reservations, order.Deliveries);
        await _billStatusService.UpdateBillStatusAsync(new UpdateBillStatusRequest
        {
            BillId = billId,
            PaymentStatus = NormalizeStatusForApi(Models.PaymentStatus.Unpaid.ToString()),
            ShipmentStatus = NormalizeStatusForApi(computedShipment)
        });

        var (paymentStatus, shipmentStatus) = await GetBillStatusOrDefault(order);

        return Ok(new AdminOrderResponse
        {
            Success = true,
            Message = "Order confirmed.",
            OrderId = order.Id,
            IsConfirmed = order.IsConfirmed,
            PaymentStatus = paymentStatus,
            ShipmentStatus = shipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt
        });
    }

    [HttpPatch("orders/{orderId}/payment")]
    public async Task<ActionResult<AdminOrderResponse>> UpdatePaymentStatus(
        int orderId,
        [FromBody] UpdateOrderStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!ValidPaymentStatuses.Contains(request.Status))
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = $"Invalid payment status. Valid values: {string.Join(", ", ValidPaymentStatuses)}."
            });
        }

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound(new AdminOrderResponse
            {
                Success = false,
                Message = "Order not found."
            });
        }

        if (order.BillId != null)
        {
            var (_, currentShipmentStatus) = await GetBillStatusOrDefault(order);
            await _billStatusService.UpdateBillStatusAsync(new UpdateBillStatusRequest
            {
                BillId = order.BillId,
                PaymentStatus = NormalizeStatusForApi(request.Status),
                ShipmentStatus = NormalizeStatusForApi(currentShipmentStatus)
            });
        }

        var (paymentStatus, shipmentStatus) = await GetBillStatusOrDefault(order);

        return Ok(new AdminOrderResponse
        {
            Success = true,
            Message = "Payment status updated.",
            OrderId = order.Id,
            IsConfirmed = order.IsConfirmed,
            PaymentStatus = paymentStatus,
            ShipmentStatus = shipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt
        });
    }

    [HttpPatch("reservations/{reservationId}/status")]
    public async Task<ActionResult> UpdateReservationStatus(
        int reservationId,
        [FromBody] UpdateStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var reservation = await _orderRepo.GetReservationByIdAsync(reservationId);
        if (reservation == null)
            return NotFound(new { Success = false, Message = "Reservation not found." });

        reservation.Picked = request.Status;
        await _orderRepo.UpdateReservationAsync(reservation);

        await SyncShipmentStatusToExternalApi(reservation.OrderId);

        return Ok(new { Success = true, Message = $"Reservation status updated." });
    }

    [HttpPatch("deliveries/{deliveryId}/status")]
    public async Task<ActionResult> UpdateDeliveryStatus(
        int deliveryId,
        [FromBody] UpdateStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var delivery = await _orderRepo.GetDeliveryByIdAsync(deliveryId);
        if (delivery == null)
            return NotFound(new { Success = false, Message = "Delivery not found." });

        delivery.Delivered = request.Status;
        await _orderRepo.UpdateDeliveryAsync(delivery);

        await SyncShipmentStatusToExternalApi(delivery.OrderId);

        return Ok(new { Success = true, Message = $"Delivery status updated." });
    }

    private async Task SyncShipmentStatusToExternalApi(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.BillId == null) return;

        var computed = ComputeShipmentStatus(order.Reservations, order.Deliveries);
        var (paymentStatus, _) = await GetBillStatusOrDefault(order);

        await _billStatusService.UpdateBillStatusAsync(new UpdateBillStatusRequest
        {
            BillId = order.BillId,
            PaymentStatus = NormalizeStatusForApi(paymentStatus),
            ShipmentStatus = NormalizeStatusForApi(computed)
        });
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<AdminProductDto>>> GetAdminProducts(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var products = await _productService.GetAllAsync();
        var hiddenProducts = await _productHiddenRepo.GetAllAsync();
        var hiddenMap = hiddenProducts.ToDictionary(h => h.ProductId, h => h.IsHidden);
        var latestPrices = await _productPriceRepo.GetLatestPerProductAsync();
        var priceMap = latestPrices.ToDictionary(p => p.ProductId);

        var result = products.Select(p => new AdminProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = priceMap.TryGetValue(p.Id, out var pp) ? pp.Price : null,
            Vat = p.Vat,
            IsHidden = hiddenMap.TryGetValue(p.Id, out var hidden) && hidden
        }).ToList();

        return Ok(result);
    }

    [HttpGet("products/{productId}")]
    public async Task<ActionResult> GetAdminProduct(
        string productId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey,
        [FromQuery] bool includeCharacteristics = false)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
            return NotFound(new { Success = false, Message = "Product not found." });

        var hidden = await _productHiddenRepo.GetByProductIdAsync(productId);
        var latestPrice = await _productPriceRepo.GetLatestForProductAsync(productId);

        var charDto = includeCharacteristics
            ? await _productCharacteristicRepo.GetByProductIdAsync(productId)
            : null;

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            price = latestPrice?.Price,
            isHidden = hidden?.IsHidden ?? false,
            characteristics = charDto
        });
    }

    [HttpPatch("products/{productId}/hidden")]
    public async Task<ActionResult> UpdateProductHidden(
        string productId,
        [FromBody] UpdateStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        await _productHiddenRepo.SetHiddenAsync(productId, request.Status);

        return Ok(new { Success = true, Message = $"Product visibility updated." });
    }

    [HttpGet("workdays")]
    public async Task<ActionResult<WorkDayResponse>> GetWorkDays(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var workDays = await _workDayRepo.GetAllAsync();
        var items = workDays.Select(w => new WorkDayInfo
        {
            Id = w.Id,
            DayName = w.DayName,
            IsWorkingDay = w.IsWorkingDay
        }).ToList();

        return Ok(new WorkDayResponse
        {
            Success = true,
            Message = "Work days retrieved.",
            WorkDays = items
        });
    }

    [HttpPatch("workdays/{id}")]
    public async Task<ActionResult<WorkDayResponse>> UpdateWorkDay(
        int id,
        [FromBody] UpdateWorkDayRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var workDay = await _workDayRepo.GetByIdAsync(id);
        if (workDay == null)
        {
            return NotFound(new WorkDayResponse
            {
                Success = false,
                Message = "Work day not found."
            });
        }

        workDay.IsWorkingDay = request.IsWorkingDay;
        await _workDayRepo.UpdateAsync(workDay);

        return Ok(new WorkDayResponse
        {
            Success = true,
            Message = "Work day updated.",
            WorkDay = new WorkDayInfo
            {
                Id = workDay.Id,
                DayName = workDay.DayName,
                IsWorkingDay = workDay.IsWorkingDay
            }
        });
    }

    [HttpGet("default-working-hours")]
    public async Task<ActionResult<DefaultWorkingHoursResponse>> GetDefaultWorkingHours(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours == null)
        {
            return NotFound(new DefaultWorkingHoursResponse
            {
                Success = false,
                Message = "Working hours not configured."
            });
        }

        return Ok(new DefaultWorkingHoursResponse
        {
            Success = true,
            Message = "Working hours retrieved.",
            WorkingHours = new DefaultWorkingHoursInfo
            {
                Id = workingHours.Id,
                StartTime = workingHours.StartTime.ToString("HH:mm"),
                EndTime = workingHours.EndTime.ToString("HH:mm")
            }
        });
    }

    [HttpPatch("default-working-hours")]
    public async Task<ActionResult<DefaultWorkingHoursResponse>> UpdateDefaultWorkingHours(
        [FromBody] UpdateDefaultWorkingHoursRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(new DefaultWorkingHoursResponse
            {
                Success = false,
                Message = "Invalid time format. Use HH:mm (e.g. 09:00)."
            });
        }

        if (startTime >= endTime)
        {
            return BadRequest(new DefaultWorkingHoursResponse
            {
                Success = false,
                Message = "Start time must be before end time."
            });
        }

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours == null)
        {
            return NotFound(new DefaultWorkingHoursResponse
            {
                Success = false,
                Message = "Working hours not configured."
            });
        }

        workingHours.StartTime = startTime;
        workingHours.EndTime = endTime;
        await _workingHoursRepo.UpdateAsync(workingHours);

        return Ok(new DefaultWorkingHoursResponse
        {
            Success = true,
            Message = "Working hours updated.",
            WorkingHours = new DefaultWorkingHoursInfo
            {
                Id = workingHours.Id,
                StartTime = workingHours.StartTime.ToString("HH:mm"),
                EndTime = workingHours.EndTime.ToString("HH:mm")
            }
        });
    }

    [HttpGet("breaks")]
    public async Task<ActionResult<DefaultBreakListResponse>> GetBreaks(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var breaks = await _breakRepo.GetAllAsync();
        return Ok(new DefaultBreakListResponse
        {
            Success = true,
            Message = "Breaks retrieved.",
            Breaks = breaks.Select(b => new DefaultBreakInfo
            {
                Id = b.Id,
                StartTime = b.StartTime.ToString("HH:mm"),
                EndTime = b.EndTime.ToString("HH:mm")
            }).ToList()
        });
    }

    [HttpPost("breaks")]
    public async Task<ActionResult<DefaultBreakResponse>> CreateBreak(
        [FromBody] UpdateDefaultWorkingHoursRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(new DefaultBreakResponse
            {
                Success = false,
                Message = "Invalid time format. Use HH:mm (e.g. 12:00)."
            });
        }

        if (startTime >= endTime)
        {
            return BadRequest(new DefaultBreakResponse
            {
                Success = false,
                Message = "Start time must be before end time."
            });
        }

        var allBreaks = await _breakRepo.GetAllAsync();

        foreach (var b in allBreaks)
        {
            if (startTime < b.EndTime && endTime > b.StartTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break overlaps with an existing break."
                });
            }
            if (endTime == b.StartTime || startTime == b.EndTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Breaks must not be directly adjacent to each other."
                });
            }
        }

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours != null)
        {
            if (startTime <= workingHours.StartTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break start time must not be earlier than working hours start."
                });
            }
            if (endTime >= workingHours.EndTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break end time must not be later than working hours end."
                });
            }
        }

        var breakItem = await _breakRepo.CreateAsync(new DefaultBreak
        {
            StartTime = startTime,
            EndTime = endTime
        });

        return Ok(new DefaultBreakResponse
        {
            Success = true,
            Message = "Break created.",
            Break = new DefaultBreakInfo
            {
                Id = breakItem.Id,
                StartTime = breakItem.StartTime.ToString("HH:mm"),
                EndTime = breakItem.EndTime.ToString("HH:mm")
            }
        });
    }

    [HttpPatch("breaks/{id}")]
    public async Task<ActionResult<DefaultBreakResponse>> UpdateBreak(
        int id,
        [FromBody] UpdateDefaultWorkingHoursRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(new DefaultBreakResponse
            {
                Success = false,
                Message = "Invalid time format. Use HH:mm (e.g. 12:00)."
            });
        }

        if (startTime >= endTime)
        {
            return BadRequest(new DefaultBreakResponse
            {
                Success = false,
                Message = "Start time must be before end time."
            });
        }

        var existing = await _breakRepo.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(new DefaultBreakResponse
            {
                Success = false,
                Message = "Break not found."
            });
        }

        var allBreaks = await _breakRepo.GetAllAsync();

        foreach (var b in allBreaks)
        {
            if (b.Id == id) continue;
            if (startTime < b.EndTime && endTime > b.StartTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break overlaps with an existing break."
                });
            }
            if (endTime == b.StartTime || startTime == b.EndTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Breaks must not be directly adjacent to each other."
                });
            }
        }

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours != null)
        {
            if (startTime <= workingHours.StartTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break start time must not be earlier than working hours start."
                });
            }
            if (endTime >= workingHours.EndTime)
            {
                return BadRequest(new DefaultBreakResponse
                {
                    Success = false,
                    Message = "Break end time must not be later than working hours end."
                });
            }
        }

        existing.StartTime = startTime;
        existing.EndTime = endTime;
        await _breakRepo.UpdateAsync(existing);

        return Ok(new DefaultBreakResponse
        {
            Success = true,
            Message = "Break updated.",
            Break = new DefaultBreakInfo
            {
                Id = existing.Id,
                StartTime = existing.StartTime.ToString("HH:mm"),
                EndTime = existing.EndTime.ToString("HH:mm")
            }
        });
    }

    [HttpDelete("breaks/{id}")]
    public async Task<ActionResult<DefaultBreakResponse>> DeleteBreak(
        int id,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var existing = await _breakRepo.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(new DefaultBreakResponse
            {
                Success = false,
                Message = "Break not found."
            });
        }

        await _breakRepo.DeleteAsync(existing);

        return Ok(new DefaultBreakResponse
        {
            Success = true,
            Message = "Break deleted."
        });
    }

    [HttpGet("minimum-booking-days")]
    public async Task<ActionResult<MinimumBookingDaysResponse>> GetMinimumBookingDays(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var minDays = await _minBookingDaysRepo.GetAsync();
        if (minDays == null)
        {
            return NotFound(new MinimumBookingDaysResponse
            {
                Success = false,
                Message = "Minimum booking days not configured."
            });
        }

        return Ok(new MinimumBookingDaysResponse
        {
            Success = true,
            Message = "Minimum booking days retrieved.",
            MinimumBookingDays = new MinimumBookingDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days,
                CountWorkingDaysOnly = minDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpPatch("minimum-booking-days")]
    public async Task<ActionResult<MinimumBookingDaysResponse>> UpdateMinimumBookingDays(
        [FromBody] UpdateMinimumBookingDaysRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.Days < 1 || request.Days > 365)
        {
            return BadRequest(new MinimumBookingDaysResponse
            {
                Success = false,
                Message = "Days must be between 1 and 365."
            });
        }

        var minDays = await _minBookingDaysRepo.GetAsync();
        if (minDays == null)
        {
            return NotFound(new MinimumBookingDaysResponse
            {
                Success = false,
                Message = "Minimum booking days not configured."
            });
        }

        minDays.Days = request.Days;
        minDays.CountWorkingDaysOnly = request.CountWorkingDaysOnly;
        await _minBookingDaysRepo.UpdateAsync(minDays);

        return Ok(new MinimumBookingDaysResponse
        {
            Success = true,
            Message = "Minimum booking days updated.",
            MinimumBookingDays = new MinimumBookingDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days,
                CountWorkingDaysOnly = minDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpGet("minimum-delivery-days")]
    public async Task<ActionResult<MinimumDeliveryDaysResponse>> GetMinimumDeliveryDays(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var minDays = await _minDeliveryDaysRepo.GetAsync();
        if (minDays == null)
        {
            return NotFound(new MinimumDeliveryDaysResponse
            {
                Success = false,
                Message = "Minimum delivery days not configured."
            });
        }

        return Ok(new MinimumDeliveryDaysResponse
        {
            Success = true,
            Message = "Minimum delivery days retrieved.",
            MinimumDeliveryDays = new MinimumDeliveryDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days,
                CountWorkingDaysOnly = minDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpPatch("minimum-delivery-days")]
    public async Task<ActionResult<MinimumDeliveryDaysResponse>> UpdateMinimumDeliveryDays(
        [FromBody] UpdateMinimumDeliveryDaysRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.Days < 1 || request.Days > 365)
        {
            return BadRequest(new MinimumDeliveryDaysResponse
            {
                Success = false,
                Message = "Days must be between 1 and 365."
            });
        }

        var minDays = await _minDeliveryDaysRepo.GetAsync();
        if (minDays == null)
        {
            return NotFound(new MinimumDeliveryDaysResponse
            {
                Success = false,
                Message = "Minimum delivery days not configured."
            });
        }

        minDays.Days = request.Days;
        minDays.CountWorkingDaysOnly = request.CountWorkingDaysOnly;
        await _minDeliveryDaysRepo.UpdateAsync(minDays);

        return Ok(new MinimumDeliveryDaysResponse
        {
            Success = true,
            Message = "Minimum delivery days updated.",
            MinimumDeliveryDays = new MinimumDeliveryDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days,
                CountWorkingDaysOnly = minDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpGet("maximum-booking-days")]
    public async Task<ActionResult<MaximumBookingDaysResponse>> GetMaximumBookingDays(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var maxDays = await _maxBookingDaysRepo.GetAsync();
        if (maxDays == null)
        {
            return NotFound(new MaximumBookingDaysResponse
            {
                Success = false,
                Message = "Maximum booking days not configured."
            });
        }

        return Ok(new MaximumBookingDaysResponse
        {
            Success = true,
            Message = "Maximum booking days retrieved.",
            MaximumBookingDays = new MaximumBookingDaysInfo
            {
                Id = maxDays.Id,
                Days = maxDays.Days,
                CountWorkingDaysOnly = maxDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpPatch("maximum-booking-days")]
    public async Task<ActionResult<MaximumBookingDaysResponse>> UpdateMaximumBookingDays(
        [FromBody] UpdateMaximumBookingDaysRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.Days < 1 || request.Days > 365)
        {
            return BadRequest(new MaximumBookingDaysResponse
            {
                Success = false,
                Message = "Days must be between 1 and 365."
            });
        }

        var maxDays = await _maxBookingDaysRepo.GetAsync();
        if (maxDays == null)
        {
            return NotFound(new MaximumBookingDaysResponse
            {
                Success = false,
                Message = "Maximum booking days not configured."
            });
        }

        maxDays.Days = request.Days;
        maxDays.CountWorkingDaysOnly = request.CountWorkingDaysOnly;
        await _maxBookingDaysRepo.UpdateAsync(maxDays);

        return Ok(new MaximumBookingDaysResponse
        {
            Success = true,
            Message = "Maximum booking days updated.",
            MaximumBookingDays = new MaximumBookingDaysInfo
            {
                Id = maxDays.Id,
                Days = maxDays.Days,
                CountWorkingDaysOnly = maxDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpGet("maximum-delivery-days")]
    public async Task<ActionResult<MaximumDeliveryDaysResponse>> GetMaximumDeliveryDays(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var maxDays = await _maxDeliveryDaysRepo.GetAsync();
        if (maxDays == null)
        {
            return NotFound(new MaximumDeliveryDaysResponse
            {
                Success = false,
                Message = "Maximum delivery days not configured."
            });
        }

        return Ok(new MaximumDeliveryDaysResponse
        {
            Success = true,
            Message = "Maximum delivery days retrieved.",
            MaximumDeliveryDays = new MaximumDeliveryDaysInfo
            {
                Id = maxDays.Id,
                Days = maxDays.Days,
                CountWorkingDaysOnly = maxDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpPatch("maximum-delivery-days")]
    public async Task<ActionResult<MaximumDeliveryDaysResponse>> UpdateMaximumDeliveryDays(
        [FromBody] UpdateMaximumDeliveryDaysRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.Days < 1 || request.Days > 365)
        {
            return BadRequest(new MaximumDeliveryDaysResponse
            {
                Success = false,
                Message = "Days must be between 1 and 365."
            });
        }

        var maxDays = await _maxDeliveryDaysRepo.GetAsync();
        if (maxDays == null)
        {
            return NotFound(new MaximumDeliveryDaysResponse
            {
                Success = false,
                Message = "Maximum delivery days not configured."
            });
        }

        maxDays.Days = request.Days;
        maxDays.CountWorkingDaysOnly = request.CountWorkingDaysOnly;
        await _maxDeliveryDaysRepo.UpdateAsync(maxDays);

        return Ok(new MaximumDeliveryDaysResponse
        {
            Success = true,
            Message = "Maximum delivery days updated.",
            MaximumDeliveryDays = new MaximumDeliveryDaysInfo
            {
                Id = maxDays.Id,
                Days = maxDays.Days,
                CountWorkingDaysOnly = maxDays.CountWorkingDaysOnly
            }
        });
    }

    [HttpPatch("allow-booking")]
    public async Task<ActionResult<AllowBookingResponse>> UpdateAllowBooking(
        [FromBody] UpdateAllowBookingRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var allowBooking = await _allowBookingRepo.GetAsync();
        if (allowBooking == null)
        {
            return NotFound(new AllowBookingResponse
            {
                Success = false,
                Message = "Allow booking not configured."
            });
        }

        allowBooking.IsAllowed = request.IsAllowed;
        await _allowBookingRepo.UpdateAsync(allowBooking);

        return Ok(new AllowBookingResponse
        {
            Success = true,
            Message = "Allow booking updated.",
            AllowBooking = new AllowBookingInfo
            {
                Id = allowBooking.Id,
                IsAllowed = allowBooking.IsAllowed
            }
        });
    }

    [HttpPatch("allow-delivery")]
    public async Task<ActionResult<AllowDeliveryResponse>> UpdateAllowDelivery(
        [FromBody] UpdateAllowDeliveryRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var allowDelivery = await _allowDeliveryRepo.GetAsync();
        if (allowDelivery == null)
        {
            return NotFound(new AllowDeliveryResponse
            {
                Success = false,
                Message = "Allow delivery not configured."
            });
        }

        allowDelivery.IsAllowed = request.IsAllowed;
        await _allowDeliveryRepo.UpdateAsync(allowDelivery);

        return Ok(new AllowDeliveryResponse
        {
            Success = true,
            Message = "Allow delivery updated.",
            AllowDelivery = new AllowDeliveryInfo
            {
                Id = allowDelivery.Id,
                IsAllowed = allowDelivery.IsAllowed
            }
        });
    }

    [HttpGet("reservation-duration")]
    public async Task<ActionResult<ReservationDurationResponse>> GetReservationDuration(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var d = await _reservationDurationRepo.GetAsync();
        if (d == null)
        {
            return NotFound(new ReservationDurationResponse
            {
                Success = false,
                Message = "Reservation duration not found."
            });
        }

        return Ok(new ReservationDurationResponse
        {
            Success = true,
            Message = "Reservation duration retrieved.",
            ReservationDuration = new ReservationDurationInfo
            {
                Id = d.Id,
                DurationMinutes = d.DurationMinutes
            }
        });
    }

    [HttpPatch("reservation-duration")]
    public async Task<ActionResult<ReservationDurationResponse>> UpdateReservationDuration(
        [FromBody] UpdateReservationDurationRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.DurationMinutes < 1 || request.DurationMinutes > 480)
        {
            return BadRequest(new ReservationDurationResponse
            {
                Success = false,
                Message = "Duration must be between 1 and 480 minutes."
            });
        }

        var d = await _reservationDurationRepo.GetAsync();
        if (d == null)
        {
            return NotFound(new ReservationDurationResponse
            {
                Success = false,
                Message = "Reservation duration not found."
            });
        }

        d.DurationMinutes = request.DurationMinutes;
        await _reservationDurationRepo.UpdateAsync(d);

        return Ok(new ReservationDurationResponse
        {
            Success = true,
            Message = "Reservation duration updated.",
            ReservationDuration = new ReservationDurationInfo
            {
                Id = d.Id,
                DurationMinutes = d.DurationMinutes
            }
        });
    }

    [HttpPost("product-prices")]
    public async Task<ActionResult<ProductPriceResponse>> AddProductPrice(
        [FromBody] AddProductPriceRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (string.IsNullOrWhiteSpace(request.ProductId))
        {
            return BadRequest(new ProductPriceResponse
            {
                Success = false,
                Message = "Product ID is required."
            });
        }

        if (request.Price < 0)
        {
            return BadRequest(new ProductPriceResponse
            {
                Success = false,
                Message = "Price must be non-negative."
            });
        }

        var price = new ProductPrice
        {
            ProductId = request.ProductId,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _productPriceRepo.AddAsync(price);

        return Ok(new ProductPriceResponse
        {
            Success = true,
            Message = "Product price added.",
            ProductPrice = new ProductPriceInfo
            {
                Id = price.Id,
                ProductId = price.ProductId,
                Price = price.Price,
                CreatedAt = price.CreatedAt
            }
        });
    }

    [HttpGet("products/{productId}/prices")]
    public async Task<ActionResult> GetProductPriceHistory(
        string productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var prices = await _productPriceRepo.GetHistoryForProductAsync(productId, from, to);
        var result = prices.Select(p => new
        {
            p.Id,
            p.ProductId,
            p.Price,
            p.CreatedAt
        }).ToList();

        return Ok(new { Success = true, Prices = result });
    }

    [HttpPost("products/{productId}/images")]
    public async Task<ActionResult> UploadProductImage(
        string productId,
        IFormFile file,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (file == null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "File is required." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var image = new ProductImage
        {
            ProductId = productId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            ImageData = ms.ToArray()
        };

        await _productImageRepo.AddAsync(image);

        var preview = _imagePreviewService.GeneratePreview(image);
        await _productImagePreviewRepo.AddAsync(preview);

        return Ok(new
        {
            Success = true,
            Message = "Image uploaded.",
            Image = new
            {
                image.Id,
                image.ProductId,
                image.FileName,
                image.ContentType,
                image.CreatedAt
            }
        });
    }

    [HttpDelete("products/{productId}/images/{imageId}")]
    public async Task<ActionResult> DeleteProductImage(
        string productId,
        int imageId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var image = await _productImageRepo.GetByIdAsync(imageId);
        if (image == null || image.ProductId != productId)
            return NotFound(new { Success = false, Message = "Image not found." });

        await _productImageRepo.DeleteAsync(image);
        await _productImagePreviewRepo.DeleteByImageIdAsync(imageId);

        return Ok(new { Success = true, Message = "Image deleted." });
    }

    [HttpPut("products/{productId}/characteristics")]
    public async Task<ActionResult> SaveProductCharacteristic(
        string productId,
        [FromBody] SaveProductCharacteristicRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var existing = await _productCharacteristicRepo.GetByProductIdAsync(productId);

        if (existing == null)
        {
            var characteristic = new ProductCharacteristic
            {
                ProductId = productId,
                SizeLengthMm = request.SizeLengthMm,
                SizeWidthMm = request.SizeWidthMm,
                SizeHeightMm = request.SizeHeightMm,
                WeightKg = request.WeightKg,
                StrengthGrade = request.StrengthGrade,
                FrostResistance = request.FrostResistance,
                WaterAbsorption = request.WaterAbsorption,
                ThermalConductivity = request.ThermalConductivity,
                RadiationQuality = request.RadiationQuality,
                QuantityPerPallet = request.QuantityPerPallet,
                Standard = request.Standard,
                Color = request.Color,
                BrickType = request.BrickType,
                MinimumOrderQuantity = request.MinimumOrderQuantity
            };

            await _productCharacteristicRepo.CreateAsync(characteristic);

            return Ok(new { Success = true, Message = "Characteristics created.", Characteristics = MapCharacteristicToDto(characteristic) });
        }

        existing.SizeLengthMm = request.SizeLengthMm;
        existing.SizeWidthMm = request.SizeWidthMm;
        existing.SizeHeightMm = request.SizeHeightMm;
        existing.WeightKg = request.WeightKg;
        existing.StrengthGrade = request.StrengthGrade;
        existing.FrostResistance = request.FrostResistance;
        existing.WaterAbsorption = request.WaterAbsorption;
        existing.ThermalConductivity = request.ThermalConductivity;
        existing.RadiationQuality = request.RadiationQuality;
        existing.QuantityPerPallet = request.QuantityPerPallet;
        existing.Standard = request.Standard;
        existing.Color = request.Color;
        existing.BrickType = request.BrickType;
        existing.MinimumOrderQuantity = request.MinimumOrderQuantity;

        await _productCharacteristicRepo.UpdateAsync(existing);

        return Ok(new { Success = true, Message = "Characteristics updated.", Characteristics = MapCharacteristicToDto(existing) });
    }

    [HttpDelete("products/{productId}/characteristics")]
    public async Task<ActionResult> DeleteProductCharacteristic(
        string productId,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var deleted = await _productCharacteristicRepo.DeleteByProductIdAsync(productId);
        if (!deleted)
            return NotFound(new { Success = false, Message = "Characteristics not found." });

        return Ok(new { Success = true, Message = "Characteristics deleted." });
    }

    [HttpGet("order-limits")]
    public async Task<ActionResult<OrderLimitsResponse>> GetOrderLimits(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var limits = await _orderLimitsRepo.GetAsync();
        if (limits == null)
        {
            return NotFound(new OrderLimitsResponse
            {
                Success = false,
                Message = "Order limits not configured."
            });
        }

        return Ok(new OrderLimitsResponse
        {
            Success = true,
            Message = "Order limits retrieved.",
            OrderLimits = new OrderLimitsInfo
            {
                Id = limits.Id,
                MinOrderPrice = limits.MinOrderPrice,
                MaxOrderPrice = limits.MaxOrderPrice,
                MinOrderQuantity = limits.MinOrderQuantity,
                MaxOrderQuantity = limits.MaxOrderQuantity,
                MinReservationQuantity = limits.MinReservationQuantity,
                MaxReservationQuantity = limits.MaxReservationQuantity,
                MinDeliveryQuantity = limits.MinDeliveryQuantity,
                MaxDeliveryQuantity = limits.MaxDeliveryQuantity,
                MinProductReservationQuantity = limits.MinProductReservationQuantity,
                MaxProductReservationQuantity = limits.MaxProductReservationQuantity
            }
        });
    }

    [HttpPatch("order-limits")]
    public async Task<ActionResult<OrderLimitsResponse>> UpdateOrderLimits(
        [FromBody] UpdateOrderLimitsRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.MinOrderPrice < 0)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MinOrderPrice must be non-negative." });
        if (request.MaxOrderPrice < request.MinOrderPrice)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MaxOrderPrice must be >= MinOrderPrice." });
        if (request.MinOrderQuantity < 0)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MinOrderQuantity must be non-negative." });
        if (request.MaxOrderQuantity < request.MinOrderQuantity)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MaxOrderQuantity must be >= MinOrderQuantity." });
        if (request.MinReservationQuantity < 0)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MinReservationQuantity must be non-negative." });
        if (request.MaxReservationQuantity < request.MinReservationQuantity)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MaxReservationQuantity must be >= MinReservationQuantity." });
        if (request.MinDeliveryQuantity < 0)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MinDeliveryQuantity must be non-negative." });
        if (request.MaxDeliveryQuantity < request.MinDeliveryQuantity)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MaxDeliveryQuantity must be >= MinDeliveryQuantity." });
        if (request.MinProductReservationQuantity < 0)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MinProductReservationQuantity must be non-negative." });
        if (request.MaxProductReservationQuantity < request.MinProductReservationQuantity)
            return BadRequest(new OrderLimitsResponse { Success = false, Message = "MaxProductReservationQuantity must be >= MinProductReservationQuantity." });

        var limits = await _orderLimitsRepo.GetAsync();
        if (limits == null)
        {
            return NotFound(new OrderLimitsResponse
            {
                Success = false,
                Message = "Order limits not configured."
            });
        }

        limits.MinOrderPrice = request.MinOrderPrice;
        limits.MaxOrderPrice = request.MaxOrderPrice;
        limits.MinOrderQuantity = request.MinOrderQuantity;
        limits.MaxOrderQuantity = request.MaxOrderQuantity;
        limits.MinReservationQuantity = request.MinReservationQuantity;
        limits.MaxReservationQuantity = request.MaxReservationQuantity;
        limits.MinDeliveryQuantity = request.MinDeliveryQuantity;
        limits.MaxDeliveryQuantity = request.MaxDeliveryQuantity;
        limits.MinProductReservationQuantity = request.MinProductReservationQuantity;
        limits.MaxProductReservationQuantity = request.MaxProductReservationQuantity;
        await _orderLimitsRepo.UpdateAsync(limits);

        return Ok(new OrderLimitsResponse
        {
            Success = true,
            Message = "Order limits updated.",
            OrderLimits = new OrderLimitsInfo
            {
                Id = limits.Id,
                MinOrderPrice = limits.MinOrderPrice,
                MaxOrderPrice = limits.MaxOrderPrice,
                MinOrderQuantity = limits.MinOrderQuantity,
                MaxOrderQuantity = limits.MaxOrderQuantity,
                MinReservationQuantity = limits.MinReservationQuantity,
                MaxReservationQuantity = limits.MaxReservationQuantity,
                MinDeliveryQuantity = limits.MinDeliveryQuantity,
                MaxDeliveryQuantity = limits.MaxDeliveryQuantity,
                MinProductReservationQuantity = limits.MinProductReservationQuantity,
                MaxProductReservationQuantity = limits.MaxProductReservationQuantity
            }
        });
    }

    [HttpGet("auto-confirm-orders")]
    public async Task<ActionResult<AutoConfirmOrdersResponse>> GetAutoConfirmOrders(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var settings = await _autoConfirmOrdersRepo.GetAsync();
        if (settings == null)
        {
            return NotFound(new AutoConfirmOrdersResponse
            {
                Success = false,
                Message = "Auto-confirm orders not configured."
            });
        }

        return Ok(new AutoConfirmOrdersResponse
        {
            Success = true,
            Message = "Auto-confirm orders retrieved.",
            AutoConfirmOrders = new AutoConfirmOrdersInfo
            {
                Id = settings.Id,
                IsEnabled = settings.IsEnabled,
                MaxAutoConfirmPrice = settings.MaxAutoConfirmPrice,
                MaxAutoConfirmQuantity = settings.MaxAutoConfirmQuantity
            }
        });
    }

    [HttpPatch("auto-confirm-orders")]
    public async Task<ActionResult<AutoConfirmOrdersResponse>> UpdateAutoConfirmOrders(
        [FromBody] UpdateAutoConfirmOrdersRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (request.MaxAutoConfirmPrice < 0)
            return BadRequest(new AutoConfirmOrdersResponse { Success = false, Message = "MaxAutoConfirmPrice must be non-negative." });
        if (request.MaxAutoConfirmQuantity < 0)
            return BadRequest(new AutoConfirmOrdersResponse { Success = false, Message = "MaxAutoConfirmQuantity must be non-negative." });

        var settings = await _autoConfirmOrdersRepo.GetAsync();
        if (settings == null)
        {
            return NotFound(new AutoConfirmOrdersResponse
            {
                Success = false,
                Message = "Auto-confirm orders not configured."
            });
        }

        settings.IsEnabled = request.IsEnabled;
        settings.MaxAutoConfirmPrice = request.MaxAutoConfirmPrice;
        settings.MaxAutoConfirmQuantity = request.MaxAutoConfirmQuantity;
        await _autoConfirmOrdersRepo.UpdateAsync(settings);

        return Ok(new AutoConfirmOrdersResponse
        {
            Success = true,
            Message = "Auto-confirm orders updated.",
            AutoConfirmOrders = new AutoConfirmOrdersInfo
            {
                Id = settings.Id,
                IsEnabled = settings.IsEnabled,
                MaxAutoConfirmPrice = settings.MaxAutoConfirmPrice,
                MaxAutoConfirmQuantity = settings.MaxAutoConfirmQuantity
            }
        });
    }

    [HttpGet("hide-products-without-price")]
    public async Task<ActionResult<HideProductsWithoutPriceResponse>> GetHideProductsWithoutPrice(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var setting = await _hideProductsWithoutPriceRepo.GetAsync();
        if (setting == null)
        {
            return NotFound(new HideProductsWithoutPriceResponse
            {
                Success = false,
                Message = "Hide products without price setting not configured."
            });
        }

        return Ok(new HideProductsWithoutPriceResponse
        {
            Success = true,
            Message = "Hide products without price setting retrieved.",
            HideProductsWithoutPrice = new HideProductsWithoutPriceInfo
            {
                Id = setting.Id,
                IsEnabled = setting.IsEnabled
            }
        });
    }

    [HttpPatch("hide-products-without-price")]
    public async Task<ActionResult<HideProductsWithoutPriceResponse>> UpdateHideProductsWithoutPrice(
        [FromBody] UpdateHideProductsWithoutPriceRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var setting = await _hideProductsWithoutPriceRepo.GetAsync();
        if (setting == null)
        {
            return NotFound(new HideProductsWithoutPriceResponse
            {
                Success = false,
                Message = "Hide products without price setting not configured."
            });
        }

        setting.IsEnabled = request.IsEnabled;
        await _hideProductsWithoutPriceRepo.UpdateAsync(setting);

        return Ok(new HideProductsWithoutPriceResponse
        {
            Success = true,
            Message = "Hide products without price setting updated.",
            HideProductsWithoutPrice = new HideProductsWithoutPriceInfo
            {
                Id = setting.Id,
                IsEnabled = setting.IsEnabled
            }
        });
    }

    [HttpGet("reservations")]
    public async Task<ActionResult> GetReservations(
        [FromQuery] DateOnly? minDate,
        [FromQuery] DateOnly? maxDate,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var reservations = await _orderRepo.GetReservationsAsync(minDate, maxDate);
        var buyers = await _buyersService.GetAllAsync();
        var buyerMap = buyers.ToDictionary(b => b.Id, b => b.Name);
        var users = await _userRepo.GetAllAsync();
        var userMap = users.ToDictionary(u => u.Id, u => u.BuyerId);

        var result = reservations.Select(r =>
        {
            var buyerId = userMap.GetValueOrDefault(r.Order.UserId);
            return new AdminReservationItem
            {
                Id = r.Id,
                Day = r.Day,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Picked = r.Picked,
                OrderId = r.OrderId,
                IsConfirmed = r.Order.IsConfirmed,
                UserBuyerId = buyerId,
                UserBuyerName = buyerId != null ? buyerMap.GetValueOrDefault(buyerId, buyerId) : null
            };
        }).ToList();

        return Ok(new AdminReservationsResponse
        {
            Success = true,
            Message = "Reservations retrieved.",
            Reservations = result
        });
    }

    [HttpGet("deliveries")]
    public async Task<ActionResult> GetDeliveries(
        [FromQuery] DateTime? minDate,
        [FromQuery] DateTime? maxDate,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var deliveries = await _orderRepo.GetDeliveriesAsync(minDate, maxDate);
        var result = deliveries.Select(d => new AdminDeliveryItem
        {
            Id = d.Id,
            DeliveryTime = d.DeliveryTime,
            Delivered = d.Delivered,
            OrderId = d.OrderId,
            IsConfirmed = d.Order.IsConfirmed
        }).ToList();

        return Ok(new AdminDeliveriesResponse
        {
            Success = true,
            Message = "Deliveries retrieved.",
            Deliveries = result
        });
    }

    private static ProductCharacteristicDto MapCharacteristicToDto(ProductCharacteristic c) => new()
    {
        Id = c.Id,
        ProductId = c.ProductId,
        SizeLengthMm = c.SizeLengthMm,
        SizeWidthMm = c.SizeWidthMm,
        SizeHeightMm = c.SizeHeightMm,
        WeightKg = c.WeightKg,
        StrengthGrade = c.StrengthGrade,
        FrostResistance = c.FrostResistance,
        WaterAbsorption = c.WaterAbsorption,
        ThermalConductivity = c.ThermalConductivity,
        RadiationQuality = c.RadiationQuality,
        QuantityPerPallet = c.QuantityPerPallet,
        Standard = c.Standard,
        Color = c.Color,
        BrickType = c.BrickType,
        MinimumOrderQuantity = c.MinimumOrderQuantity
    };
}
