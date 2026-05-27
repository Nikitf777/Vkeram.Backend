using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Data;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.ContactEmail == request.ContactEmail))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "A user with this email already exists."
            });
        }

        var invite = await _db.InviteCodes
            .FirstOrDefaultAsync(i => i.Code == request.InviteCode && !i.IsUsed);

        if (invite == null)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Invalid or already used invite code."
            });
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "This invite code has expired."
            });
        }

        var user = new User
        {
            CompanyName = request.CompanyName,
            ContactEmail = request.ContactEmail,
            ContactName = request.ContactName,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        invite.IsUsed = true;
        invite.UsedByUserId = user.Id;
        invite.UsedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Registration successful.",
            UserId = user.Id,
            CompanyName = user.CompanyName
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.ContactEmail == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            });
        }

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.Id,
            CompanyName = user.CompanyName
        });
    }
}
