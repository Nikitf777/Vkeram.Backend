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

        var slots = requests.Select(r =>
        {
            if (r.StartTime.Kind == DateTimeKind.Unspecified)
                r.StartTime = DateTime.SpecifyKind(r.StartTime, DateTimeKind.Utc);
            return (StartTime: r.StartTime, EndTime: r.StartTime.AddMinutes(30));
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
            order.Reservations.Add(new OrderReservation
            {
                StartTime = slot.StartTime,
                EndTime = slot.EndTime
            });
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
                EndTime = r.EndTime
            }).ToList()
        });
    }
}
