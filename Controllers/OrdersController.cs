using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductService _productService;
    private readonly IWorkDayRepository _workDayRepo;
    private readonly IDefaultWorkingHoursRepository _workingHoursRepo;
    private readonly IDefaultBreakRepository _breakRepo;
    private readonly IMinimumBookingDaysRepository _minBookingDaysRepo;
    private readonly IMinimumDeliveryDaysRepository _minDeliveryDaysRepo;
    private readonly IProductPriceRepository _productPriceRepo;
    private readonly IReservationDurationRepository _reservationDurationRepo;
    private readonly IAllowBookingRepository _allowBookingRepo;
    private readonly IAllowDeliveryRepository _allowDeliveryRepo;
    private readonly IOrderLimitsRepository _orderLimitsRepo;
    private readonly IAutoConfirmOrdersRepository _autoConfirmOrdersRepo;
    private readonly IBillsService _billsService;
    private readonly IUserRepository _userRepo;

    public OrdersController(IOrderRepository orderRepo, IProductService productService, IWorkDayRepository workDayRepo, IDefaultWorkingHoursRepository workingHoursRepo, IDefaultBreakRepository breakRepo, IMinimumBookingDaysRepository minBookingDaysRepo, IMinimumDeliveryDaysRepository minDeliveryDaysRepo, IProductPriceRepository productPriceRepo, IReservationDurationRepository reservationDurationRepo, IAllowBookingRepository allowBookingRepo, IAllowDeliveryRepository allowDeliveryRepo, IOrderLimitsRepository orderLimitsRepo, IAutoConfirmOrdersRepository autoConfirmOrdersRepo, IBillsService billsService, IUserRepository userRepo)
    {
        _orderRepo = orderRepo;
        _productService = productService;
        _workDayRepo = workDayRepo;
        _workingHoursRepo = workingHoursRepo;
        _breakRepo = breakRepo;
        _minBookingDaysRepo = minBookingDaysRepo;
        _minDeliveryDaysRepo = minDeliveryDaysRepo;
        _productPriceRepo = productPriceRepo;
        _reservationDurationRepo = reservationDurationRepo;
        _allowBookingRepo = allowBookingRepo;
        _allowDeliveryRepo = allowDeliveryRepo;
        _orderLimitsRepo = orderLimitsRepo;
        _autoConfirmOrdersRepo = autoConfirmOrdersRepo;
        _billsService = billsService;
        _userRepo = userRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var orders = await _orderRepo.GetByUserIdAsync(userId);

        var allProductIds = orders
            .SelectMany(o => o.Reservations.SelectMany(r => r.ProductReservations.Select(p => p.ProductId)))
            .Concat(orders.SelectMany(o => o.Deliveries.SelectMany(d => d.ProductReservations.Select(p => p.ProductId))))
            .Distinct()
            .ToList();

        var productMap = allProductIds.Count > 0
            ? await _productService.GetByIdsAsync(allProductIds)
            : new Dictionary<string, ProductDto>();

        var result = orders.Select(o =>
        {
            var reservations = o.Reservations.Select(r => new ReservationInfo
            {
                Id = r.Id,
                StartTime = r.Day.ToDateTime(r.StartTime),
                EndTime = r.Day.ToDateTime(r.EndTime),
                Products = r.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                    Quantity = pr.Quantity,
                    Price = pr.ProductPrice?.Price ?? 0,
                    TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity
                }).ToList()
            }).ToList();

            var deliveries = o.Deliveries.Select(d => new DeliveryInfo
            {
                Id = d.Id,
                DeliveryTime = d.DeliveryTime,
                Products = d.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                    Quantity = pr.Quantity,
                    Price = pr.ProductPrice?.Price ?? 0,
                    TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity
                }).ToList()
            }).ToList();

            return new OrderResponse
            {
                Success = true,
                Message = "Order retrieved.",
                OrderId = o.Id,
                IsConfirmed = o.IsConfirmed,
                PaymentStatus = o.PaymentStatus,
                ShipmentStatus = o.ShipmentStatus,
                UserId = o.UserId,
                CreatedAt = o.CreatedAt,
                Reservations = reservations,
                Deliveries = deliveries,
                TotalPrice = o.TotalPrice,
                TotalQuantity = o.TotalQuantity
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetMyOrderById(int orderId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var o = await _orderRepo.GetByIdAsync(orderId);
        if (o == null || o.UserId != userId)
            return NotFound(new OrderResponse { Success = false, Message = "Order not found." });

        var allProductIds = o.Reservations
            .SelectMany(r => r.ProductReservations.Select(p => p.ProductId))
            .Concat(o.Deliveries.SelectMany(d => d.ProductReservations.Select(p => p.ProductId)))
            .Distinct()
            .ToList();

        var productMap = allProductIds.Count > 0
            ? await _productService.GetByIdsAsync(allProductIds)
            : new Dictionary<string, ProductDto>();

        var reservations = o.Reservations.Select(r => new ReservationInfo
        {
            Id = r.Id,
            StartTime = r.Day.ToDateTime(r.StartTime),
            EndTime = r.Day.ToDateTime(r.EndTime),
            Products = r.ProductReservations.Select(pr => new ProductReservationInfo
            {
                ProductId = pr.ProductId,
                ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                Quantity = pr.Quantity,
                Price = pr.ProductPrice?.Price ?? 0,
                TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity
            }).ToList()
        }).ToList();

        var deliveries = o.Deliveries.Select(d => new DeliveryInfo
        {
            Id = d.Id,
            DeliveryTime = d.DeliveryTime,
            Products = d.ProductReservations.Select(pr => new ProductReservationInfo
            {
                ProductId = pr.ProductId,
                ProductName = productMap.TryGetValue(pr.ProductId, out var p) ? p.Name : pr.ProductId,
                Quantity = pr.Quantity,
                Price = pr.ProductPrice?.Price ?? 0,
                TotalPrice = (pr.ProductPrice?.Price ?? 0) * pr.Quantity
            }).ToList()
        }).ToList();

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order retrieved.",
            OrderId = o.Id,
            IsConfirmed = o.IsConfirmed,
            PaymentStatus = o.PaymentStatus,
            ShipmentStatus = o.ShipmentStatus,
            UserId = o.UserId,
            CreatedAt = o.CreatedAt,
            Reservations = reservations,
            Deliveries = deliveries,
            TotalPrice = o.TotalPrice,
            TotalQuantity = o.TotalQuantity
        });
    }

    [HttpGet("reservation-duration")]
    public async Task<ActionResult<ReservationDurationResponse>> GetReservationDuration()
    {
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

    [HttpGet("reservations")]
    public async Task<ActionResult<List<ReservationSlotInfo>>> GetFutureReservations(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest(new { Success = false, Message = "'from' must not be later than 'to'." });
        }

        if (from.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (from.Value < today)
            {
                return BadRequest(new { Success = false, Message = "'from' must not be in the past." });
            }

        var workingHours = await _workingHoursRepo.GetAsync();
        var breaks = await _breakRepo.GetAllAsync();
            if (from.Value == today && workingHours != null && TimeOnly.FromDateTime(DateTime.UtcNow) > workingHours.StartTime)
            {
                return BadRequest(new { Success = false, Message = "'from' cannot be today because working hours have already started." });
            }
        }

        var reservations = await _orderRepo.GetFutureReservationsAsync(from, to);
        var slots = reservations.Select(r => new ReservationSlotInfo
        {
            StartTime = r.Day.ToDateTime(r.StartTime),
            EndTime = r.Day.ToDateTime(r.EndTime)
        }).ToList();
        return Ok(slots);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderPayload payload)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (payload.Reservations.Count == 0 && payload.Deliveries.Count == 0)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = "At least one reservation or one delivery is required."
            });
        }

        var allProductIds = payload.Reservations
            .SelectMany(r => r.Products.Select(p => p.ProductId))
            .Concat(payload.Deliveries.SelectMany(d => d.Products.Select(p => p.ProductId)))
            .Distinct().ToList();
        var existingProducts = await _productService.GetByIdsAsync(allProductIds);

        foreach (var unknownId in allProductIds.Where(id => !existingProducts.ContainsKey(id)))
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = $"Product with ID {unknownId} does not exist."
            });
        }

        var latestPrices = await _productPriceRepo.GetLatestPerProductAsync();
        var priceMap = latestPrices.ToDictionary(p => p.ProductId);
        foreach (var pid in allProductIds)
        {
            if (!priceMap.ContainsKey(pid))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Product with ID {pid} does not have a price set."
                });
            }
        }

        foreach (var r in payload.Reservations)
        {
            if (r.Products.Count == 0)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Each reservation must have at least one product."
                });
            }

            if (r.Products.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Duplicate products are not allowed within a reservation."
                });
            }
        }

        foreach (var d in payload.Deliveries)
        {
            if (d.Products.Count == 0)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Each delivery must have at least one product."
                });
            }

            if (d.Products.GroupBy(p => p.ProductId).Any(g => g.Count() > 1))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Duplicate products are not allowed within a delivery."
                });
            }
        }

        var workDays = await _workDayRepo.GetAllAsync();
        var workingDayNames = workDays.Where(w => w.IsWorkingDay).Select(w => w.DayName).ToHashSet();

        var workingHours = await _workingHoursRepo.GetAsync();
        var breaks = await _breakRepo.GetAllAsync();

        var minBookingDays = await _minBookingDaysRepo.GetAsync();
        DateOnly? earliestReservationDate = null;
        if (minBookingDays != null)
            earliestReservationDate = GetEarliestAllowedDate(DateTime.UtcNow, minBookingDays.Days, minBookingDays.CountWorkingDaysOnly, workingHours?.StartTime, workingDayNames);

        var minDeliveryDays = await _minDeliveryDaysRepo.GetAsync();
        DateOnly? earliestDeliveryDate = null;
        if (minDeliveryDays != null)
            earliestDeliveryDate = GetEarliestAllowedDate(DateTime.UtcNow, minDeliveryDays.Days, minDeliveryDays.CountWorkingDaysOnly, workingHours?.StartTime, workingDayNames);

        var reservationDuration = (await _reservationDurationRepo.GetAsync())?.DurationMinutes ?? 30;

        var slots = payload.Reservations.Select(r =>
        {
            if (r.StartTime.Kind == DateTimeKind.Unspecified)
                r.StartTime = DateTime.SpecifyKind(r.StartTime, DateTimeKind.Utc);
            var day = DateOnly.FromDateTime(r.StartTime);
            var startTime = TimeOnly.FromDateTime(r.StartTime);
            var endTime = startTime.AddMinutes(reservationDuration);
            return (Day: day, StartTime: startTime, EndTime: endTime, Products: r.Products);
        }).ToList();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        if (payload.Reservations.Count > 0)
        {
            var allowBooking = await _allowBookingRepo.GetAsync();
            if (allowBooking != null && !allowBooking.IsAllowed)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Booking is currently not allowed."
                });
            }
        }

        foreach (var slot in slots)
        {
            if (slot.Day < today || (slot.Day == today && slot.StartTime <= currentTime))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Reservation start time must be in the future."
                });
            }

            if (!workingDayNames.Contains(slot.Day.DayOfWeek.ToString()))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Reservations can only be made on working days."
                });
            }

            if (earliestReservationDate.HasValue && slot.Day < earliestReservationDate.Value)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Reservations must be at least {minBookingDays!.Days} day(s) in advance."
                });
            }

            if (workingHours != null)
            {
                if (slot.StartTime < workingHours.StartTime || slot.EndTime > workingHours.EndTime)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = $"Reservations must be within working hours ({workingHours.StartTime:HH:mm}-{workingHours.EndTime:HH:mm})."
                    });
                }
            }

            foreach (var b in breaks)
            {
                if (slot.StartTime < b.EndTime && slot.EndTime > b.StartTime)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = "Reservations must not overlap with breaks."
                    });
                }
            }

            if (await _orderRepo.HasOverlappingReservationAsync(slot.Day, slot.StartTime, slot.EndTime, reservationDuration))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "The requested time slot is too close to an existing reservation or overlaps."
                });
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            for (int j = i + 1; j < slots.Count; j++)
            {
                var si = slots[i];
                var sj = slots[j];
                var siEnd = si.EndTime.AddMinutes(reservationDuration);
                var sjEnd = sj.EndTime.AddMinutes(reservationDuration);
                if (si.Day == sj.Day && si.StartTime < sjEnd && sj.StartTime < siEnd)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = "The requested time slots are too close to each other."
                    });
                }
            }
        }

        var order = new Order
        {
            UserId = userId,
            IsConfirmed = false,
            PaymentStatus = Models.PaymentStatus.Unpaid.ToString()
        };

        foreach (var slot in slots)
        {
            var reservation = new OrderReservation
            {
                Day = slot.Day,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime
            };

            foreach (var productReq in slot.Products)
            {
                reservation.ProductReservations.Add(new ProductReservation
                {
                    ProductId = productReq.ProductId,
                    Quantity = productReq.Quantity,
                    ProductPriceId = priceMap[productReq.ProductId].Id
                });
            }

            order.Reservations.Add(reservation);
        }

        if (payload.Deliveries.Count > 0)
        {
            var allowDelivery = await _allowDeliveryRepo.GetAsync();
            if (allowDelivery != null && !allowDelivery.IsAllowed)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Delivery is currently not allowed."
                });
            }
        }

        foreach (var deliveryReq in payload.Deliveries)
        {
            if (deliveryReq.DeliveryTime.Kind == DateTimeKind.Unspecified)
                deliveryReq.DeliveryTime = DateTime.SpecifyKind(deliveryReq.DeliveryTime, DateTimeKind.Utc);

            if (!workingDayNames.Contains(deliveryReq.DeliveryTime.DayOfWeek.ToString()))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Deliveries can only be scheduled on working days."
                });
            }

            if (earliestDeliveryDate.HasValue && DateOnly.FromDateTime(deliveryReq.DeliveryTime) < earliestDeliveryDate.Value)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = $"Deliveries must be at least {minDeliveryDays!.Days} day(s) in advance."
                });
            }

            if (workingHours != null)
            {
                var deliveryTime = TimeOnly.FromDateTime(deliveryReq.DeliveryTime);

                if (deliveryTime < workingHours.StartTime || deliveryTime > workingHours.EndTime)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = $"Deliveries must be scheduled within working hours ({workingHours.StartTime:HH:mm}-{workingHours.EndTime:HH:mm})."
                    });
                }
            }

            var delivery = new OrderDelivery
            {
                DeliveryTime = deliveryReq.DeliveryTime
            };

            foreach (var productReq in deliveryReq.Products)
            {
                delivery.ProductReservations.Add(new ProductReservation
                {
                    ProductId = productReq.ProductId,
                    Quantity = productReq.Quantity,
                    ProductPriceId = priceMap[productReq.ProductId].Id
                });
            }

            order.Deliveries.Add(delivery);
        }

        var autoConfirmSettings = await _autoConfirmOrdersRepo.GetAsync();
        var shouldAutoConfirm = false;

        if (autoConfirmSettings != null && autoConfirmSettings.IsEnabled)
        {
            shouldAutoConfirm = order.TotalPrice <= autoConfirmSettings.MaxAutoConfirmPrice
                && order.TotalQuantity <= autoConfirmSettings.MaxAutoConfirmQuantity;
        }

        if (shouldAutoConfirm)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
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
                    BuyerId = user.BuyerId,
                    Products = billProducts
                };

                var billId = await _billsService.CreateBillAsync(billRequest);
                if (billId != null)
                {
                    order.IsConfirmed = true;
                    order.BillId = billId;
                }
            }
        }

        await _orderRepo.CreateAsync(order);

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order created successfully.",
            OrderId = order.Id,
            IsConfirmed = order.IsConfirmed,
            PaymentStatus = order.PaymentStatus,
            ShipmentStatus = order.ShipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt,
            Reservations = order.Reservations.Select(r =>
            {
                var products = r.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = existingProducts[pr.ProductId].Name,
                    Quantity = pr.Quantity,
                    Price = priceMap[pr.ProductId].Price,
                    TotalPrice = priceMap[pr.ProductId].Price * pr.Quantity
                }).ToList();
                return new ReservationInfo
                {
                    Id = r.Id,
                    StartTime = r.Day.ToDateTime(r.StartTime),
                    EndTime = r.Day.ToDateTime(r.EndTime),
                    Products = products
                };
            }).ToList(),
            Deliveries = order.Deliveries.Select(d =>
            {
                var products = d.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = existingProducts[pr.ProductId].Name,
                    Quantity = pr.Quantity,
                    Price = priceMap[pr.ProductId].Price,
                    TotalPrice = priceMap[pr.ProductId].Price * pr.Quantity
                }).ToList();
                return new DeliveryInfo
                {
                    Id = d.Id,
                    DeliveryTime = d.DeliveryTime,
                    Products = products
                };
            }).ToList(),
            TotalPrice = order.Reservations.Sum(r => r.ProductReservations.Sum(pr => priceMap[pr.ProductId].Price * pr.Quantity))
                       + order.Deliveries.Sum(d => d.ProductReservations.Sum(pr => priceMap[pr.ProductId].Price * pr.Quantity)),
            TotalQuantity = order.Reservations.Sum(r => r.ProductReservations.Sum(pr => pr.Quantity))
                          + order.Deliveries.Sum(d => d.ProductReservations.Sum(pr => pr.Quantity))
        });
    }

    [HttpGet("breaks")]
    public async Task<ActionResult<DefaultBreakListResponse>> GetBreaks()
    {
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

    [HttpGet("allow-booking")]
    public async Task<ActionResult<AllowBookingResponse>> GetAllowBooking()
    {
        var allowBooking = await _allowBookingRepo.GetAsync();
        if (allowBooking == null)
        {
            return NotFound(new AllowBookingResponse
            {
                Success = false,
                Message = "Allow booking not configured."
            });
        }

        return Ok(new AllowBookingResponse
        {
            Success = true,
            Message = "Allow booking retrieved.",
            AllowBooking = new AllowBookingInfo
            {
                Id = allowBooking.Id,
                IsAllowed = allowBooking.IsAllowed
            }
        });
    }

    [HttpGet("allow-delivery")]
    public async Task<ActionResult<AllowDeliveryResponse>> GetAllowDelivery()
    {
        var allowDelivery = await _allowDeliveryRepo.GetAsync();
        if (allowDelivery == null)
        {
            return NotFound(new AllowDeliveryResponse
            {
                Success = false,
                Message = "Allow delivery not configured."
            });
        }

        return Ok(new AllowDeliveryResponse
        {
            Success = true,
            Message = "Allow delivery retrieved.",
            AllowDelivery = new AllowDeliveryInfo
            {
                Id = allowDelivery.Id,
                IsAllowed = allowDelivery.IsAllowed
            }
        });
    }

    [HttpGet("order-limits")]
    public async Task<ActionResult<OrderLimitsResponse>> GetOrderLimits()
    {
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

    [HttpGet("auto-confirm-orders")]
    public async Task<ActionResult<AutoConfirmOrdersResponse>> GetAutoConfirmOrders()
    {
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

    private static DateOnly GetEarliestAllowedDate(DateTime now, int days, bool countWorkingDaysOnly, TimeOnly? workingHoursStart, HashSet<string> workingDayNames)
    {
        DateOnly startDate;
        if (workingHoursStart != null && TimeOnly.FromDateTime(now) > workingHoursStart)
            startDate = DateOnly.FromDateTime(now.Date.AddDays(1));
        else
            startDate = DateOnly.FromDateTime(now.Date);

        if (!countWorkingDaysOnly)
            return startDate.AddDays(days - 1);

        var current = startDate;
        var counted = 0;
        while (counted < days)
        {
            if (workingDayNames.Contains(current.DayOfWeek.ToString()))
                counted++;
            if (counted < days)
                current = current.AddDays(1);
        }
        return current;
    }
}
