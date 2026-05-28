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
    private readonly IWorkingHoursRepository _workingHoursRepo;

    public OrdersController(IOrderRepository orderRepo, IProductService productService, IWorkDayRepository workDayRepo, IWorkingHoursRepository workingHoursRepo)
    {
        _orderRepo = orderRepo;
        _productService = productService;
        _workDayRepo = workDayRepo;
        _workingHoursRepo = workingHoursRepo;
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<List<ReservationSlotInfo>>> GetFutureReservations()
    {
        var reservations = await _orderRepo.GetFutureReservationsAsync();
        var slots = reservations.Select(r => new ReservationSlotInfo
        {
            StartTime = r.StartTime,
            EndTime = r.EndTime
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

        var slots = payload.Reservations.Select(r =>
        {
            if (r.StartTime.Kind == DateTimeKind.Unspecified)
                r.StartTime = DateTime.SpecifyKind(r.StartTime, DateTimeKind.Utc);
            return (StartTime: r.StartTime, EndTime: r.StartTime.AddMinutes(30), Products: r.Products);
        }).ToList();

        foreach (var slot in slots)
        {
            if (slot.StartTime <= DateTime.UtcNow)
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Reservation start time must be in the future."
                });
            }

            if (!workingDayNames.Contains(slot.StartTime.DayOfWeek.ToString()))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "Reservations can only be made on working days."
                });
            }

            if (workingHours != null)
            {
                var slotStart = TimeOnly.FromDateTime(slot.StartTime);
                var slotEnd = TimeOnly.FromDateTime(slot.EndTime);

                if (slotStart < workingHours.StartTime || slotEnd > workingHours.EndTime)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = $"Reservations must be within working hours ({workingHours.StartTime:HH:mm}-{workingHours.EndTime:HH:mm})."
                    });
                }
            }

            if (await _orderRepo.HasOverlappingReservationAsync(slot.StartTime, slot.EndTime))
            {
                return BadRequest(new OrderResponse
                {
                    Success = false,
                    Message = "The requested time slot overlaps with an existing reservation."
                });
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            for (int j = i + 1; j < slots.Count; j++)
            {
                if (slots[i].StartTime < slots[j].EndTime && slots[j].StartTime < slots[i].EndTime)
                {
                    return BadRequest(new OrderResponse
                    {
                        Success = false,
                        Message = "The requested time slots overlap with each other."
                    });
                }
            }
        }

        var order = new Order
        {
            UserId = userId,
            ConfirmationStatus = Models.ConfirmationStatus.Confirmed.ToString(),
            PaymentStatus = Models.PaymentStatus.Unpaid.ToString(),
            ShipmentStatus = Models.ShipmentStatus.Unshipped.ToString()
        };

        foreach (var slot in slots)
        {
            var reservation = new OrderReservation
            {
                StartTime = slot.StartTime,
                EndTime = slot.EndTime
            };

            foreach (var productReq in slot.Products)
            {
                reservation.ProductReservations.Add(new ProductReservation
                {
                    ProductId = productReq.ProductId,
                    Quantity = productReq.Quantity
                });
            }

            order.Reservations.Add(reservation);
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
                    Quantity = productReq.Quantity
                });
            }

            order.Deliveries.Add(delivery);
        }

        await _orderRepo.CreateAsync(order);

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order created successfully.",
            OrderId = order.Id,
            ConfirmationStatus = order.ConfirmationStatus,
            PaymentStatus = order.PaymentStatus,
            ShipmentStatus = order.ShipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt,
            Reservations = order.Reservations.Select(r => new ReservationInfo
            {
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Products = r.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = existingProducts[pr.ProductId].Name,
                    Quantity = pr.Quantity
                }).ToList()
            }).ToList(),
            Deliveries = order.Deliveries.Select(d => new DeliveryInfo
            {
                DeliveryTime = d.DeliveryTime,
                Products = d.ProductReservations.Select(pr => new ProductReservationInfo
                {
                    ProductId = pr.ProductId,
                    ProductName = existingProducts[pr.ProductId].Name,
                    Quantity = pr.Quantity
                }).ToList()
            }).ToList()
        });
    }
}
