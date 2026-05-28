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
    private readonly IWorkDayRepository _workDayRepo;
    private readonly IWorkingHoursRepository _workingHoursRepo;
    private readonly IMinimumBookingDaysRepository _minBookingDaysRepo;
    private readonly IMinimumDeliveryDaysRepository _minDeliveryDaysRepo;
    private readonly string _adminKey;

    public AdminController(IInviteCodeRepository inviteRepo, IOrderRepository orderRepo, IWorkDayRepository workDayRepo, IWorkingHoursRepository workingHoursRepo, IMinimumBookingDaysRepository minBookingDaysRepo, IMinimumDeliveryDaysRepository minDeliveryDaysRepo, IConfiguration config)
    {
        _inviteRepo = inviteRepo;
        _orderRepo = orderRepo;
        _workDayRepo = workDayRepo;
        _workingHoursRepo = workingHoursRepo;
        _minBookingDaysRepo = minBookingDaysRepo;
        _minDeliveryDaysRepo = minDeliveryDaysRepo;
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

    [HttpGet("working-hours")]
    public async Task<ActionResult<WorkingHoursResponse>> GetWorkingHours(
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours == null)
        {
            return NotFound(new WorkingHoursResponse
            {
                Success = false,
                Message = "Working hours not configured."
            });
        }

        return Ok(new WorkingHoursResponse
        {
            Success = true,
            Message = "Working hours retrieved.",
            WorkingHours = new WorkingHoursInfo
            {
                Id = workingHours.Id,
                StartTime = workingHours.StartTime.ToString("HH:mm"),
                EndTime = workingHours.EndTime.ToString("HH:mm")
            }
        });
    }

    [HttpPatch("working-hours")]
    public async Task<ActionResult<WorkingHoursResponse>> UpdateWorkingHours(
        [FromBody] UpdateWorkingHoursRequest request,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (!IsValidAdmin(adminKey)) return UnauthorizedResponse();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest(new WorkingHoursResponse
            {
                Success = false,
                Message = "Invalid time format. Use HH:mm (e.g. 09:00)."
            });
        }

        if (startTime >= endTime)
        {
            return BadRequest(new WorkingHoursResponse
            {
                Success = false,
                Message = "Start time must be before end time."
            });
        }

        var workingHours = await _workingHoursRepo.GetAsync();
        if (workingHours == null)
        {
            return NotFound(new WorkingHoursResponse
            {
                Success = false,
                Message = "Working hours not configured."
            });
        }

        workingHours.StartTime = startTime;
        workingHours.EndTime = endTime;
        await _workingHoursRepo.UpdateAsync(workingHours);

        return Ok(new WorkingHoursResponse
        {
            Success = true,
            Message = "Working hours updated.",
            WorkingHours = new WorkingHoursInfo
            {
                Id = workingHours.Id,
                StartTime = workingHours.StartTime.ToString("HH:mm"),
                EndTime = workingHours.EndTime.ToString("HH:mm")
            }
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
                Days = minDays.Days
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
        await _minBookingDaysRepo.UpdateAsync(minDays);

        return Ok(new MinimumBookingDaysResponse
        {
            Success = true,
            Message = "Minimum booking days updated.",
            MinimumBookingDays = new MinimumBookingDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days
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
                Days = minDays.Days
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
        await _minDeliveryDaysRepo.UpdateAsync(minDays);

        return Ok(new MinimumDeliveryDaysResponse
        {
            Success = true,
            Message = "Minimum delivery days updated.",
            MinimumDeliveryDays = new MinimumDeliveryDaysInfo
            {
                Id = minDays.Id,
                Days = minDays.Days
            }
        });
    }
}
