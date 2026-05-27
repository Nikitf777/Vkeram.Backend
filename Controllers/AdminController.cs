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
    private readonly string _adminKey;

    public AdminController(IInviteCodeRepository inviteRepo, IConfiguration config)
    {
        _inviteRepo = inviteRepo;
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
}
