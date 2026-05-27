using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.DTOs;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IInviteCodeRepository _inviteRepo;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepo, IInviteCodeRepository inviteRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        _inviteRepo = inviteRepo;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (await _userRepo.ExistsByEmailAsync(request.ContactEmail))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "A user with this email already exists."
            });
        }

        var invite = await _inviteRepo.GetByCodeAsync(request.InviteCode);

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

        await _userRepo.CreateAsync(user);
        await _inviteRepo.MarkAsUsedAsync(invite, user.Id);

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Registration successful.",
            UserId = user.Id,
            CompanyName = user.CompanyName,
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            });
        }

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.Id,
            CompanyName = user.CompanyName,
            Token = token
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.ContactEmail),
            new Claim("companyName", user.CompanyName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"] ?? "20")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AuthResponse> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var companyName = User.FindFirst("companyName")?.Value;

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Token is valid.",
            UserId = int.Parse(userId!),
            CompanyName = companyName,
            Token = Request.Headers.Authorization.ToString().Replace("Bearer ", "")
        });
    }
}
