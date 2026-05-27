using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Data;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] List<CreateOrderRequest> requests)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (requests.Count == 0)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = "At least one reservation is required."
            });
        }

        var allProductIds = requests.SelectMany(r => r.Products.Select(p => p.ProductId)).Distinct().ToList();
        var existingProducts = await _db.Products.Where(p => allProductIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        foreach (var unknownId in allProductIds.Where(id => !existingProducts.ContainsKey(id)))
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = $"Product with ID {unknownId} does not exist."
            });
        }

        foreach (var r in requests)
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

        var slots = requests.Select(r =>
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

            if (await _db.OrderReservations
                .AnyAsync(r => r.StartTime < slot.EndTime && slot.StartTime < r.EndTime))
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
            Status = OrderStatus.PENDING_PAYMENT.ToString()
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

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new OrderResponse
        {
            Success = true,
            Message = "Order created successfully.",
            OrderId = order.Id,
            Status = order.Status,
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
            }).ToList()
        });
    }
}
