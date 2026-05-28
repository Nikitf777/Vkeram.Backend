using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IInviteCodeRepository _inviteRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly string _adminKey;

    public AdminController(IInviteCodeRepository inviteRepo, IOrderRepository orderRepo, IConfiguration config)
    {
        _inviteRepo = inviteRepo;
        _orderRepo = orderRepo;
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
                CompanyName = request.CompanyName,
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
        var invites = inviteList.Select(i => new
        {
            i.Id,
            i.Code,
            i.CompanyName,
            i.IsUsed,
            i.UsedByUserId,
            i.CreatedAt,
            i.UsedAt,
            i.ExpiresAt
        }).ToList();

        return Ok(new { Success = true, Invites = invites });
    }

    private bool IsValidAdmin(string adminKey) => adminKey == _adminKey;

    private ActionResult UnauthorizedResponse() =>
        Unauthorized(new { Success = false, Message = "Invalid admin key." });

    private static readonly string[] ValidConfirmationStatuses = ["Confirmed", "Unconfirmed", "Cancelled"];
    private static readonly string[] ValidPaymentStatuses = ["Paid", "PartiallyPaid", "Unpaid"];
    private static readonly string[] ValidShipmentStatuses = ["Shipped", "PartiallyShipped", "Unshipped"];

    [HttpPatch("orders/{orderId}/confirmation")]
    public async Task<ActionResult<AdminOrderResponse>> UpdateConfirmationStatus(
        int orderId,
        [FromBody] UpdateOrderStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!ValidConfirmationStatuses.Contains(request.Status))
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = $"Invalid confirmation status. Valid values: {string.Join(", ", ValidConfirmationStatuses)}."
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

        order.ConfirmationStatus = request.Status;
        await _orderRepo.UpdateAsync(order);

        return Ok(new AdminOrderResponse
        {
            Success = true,
            Message = "Confirmation status updated.",
            OrderId = order.Id,
            ConfirmationStatus = order.ConfirmationStatus,
            PaymentStatus = order.PaymentStatus,
            ShipmentStatus = order.ShipmentStatus,
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

        order.PaymentStatus = request.Status;
        await _orderRepo.UpdateAsync(order);

        return Ok(new AdminOrderResponse
        {
            Success = true,
            Message = "Payment status updated.",
            OrderId = order.Id,
            ConfirmationStatus = order.ConfirmationStatus,
            PaymentStatus = order.PaymentStatus,
            ShipmentStatus = order.ShipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt
        });
    }

    [HttpPatch("orders/{orderId}/shipment")]
    public async Task<ActionResult<AdminOrderResponse>> UpdateShipmentStatus(
        int orderId,
        [FromBody] UpdateOrderStatusRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!ValidShipmentStatuses.Contains(request.Status))
        {
            return BadRequest(new AdminOrderResponse
            {
                Success = false,
                Message = $"Invalid shipment status. Valid values: {string.Join(", ", ValidShipmentStatuses)}."
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

        order.ShipmentStatus = request.Status;
        await _orderRepo.UpdateAsync(order);

        return Ok(new AdminOrderResponse
        {
            Success = true,
            Message = "Shipment status updated.",
            OrderId = order.Id,
            ConfirmationStatus = order.ConfirmationStatus,
            PaymentStatus = order.PaymentStatus,
            ShipmentStatus = order.ShipmentStatus,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt
        });
    }
}
