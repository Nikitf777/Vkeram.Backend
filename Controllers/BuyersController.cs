using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Vkeram.Backend.Data.Repositories;
using Vkeram.Backend.Services;

namespace Vkeram.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BuyersController : ControllerBase
{
    private readonly IBuyersService _buyersService;
    private readonly IUserRepository _userRepo;
    private readonly string _adminKey;

    public BuyersController(IBuyersService buyersService, IUserRepository userRepo, IConfiguration config)
    {
        _buyersService = buyersService;
        _userRepo = userRepo;
        _adminKey = config["AdminApiKey"] ?? "";
    }

    [HttpGet]
    public async Task<ActionResult> GetBuyers(
        [FromHeader(Name = "X-Admin-Key")] string adminKey,
        [FromQuery] bool onlyWithUsers = false)
    {
        if (adminKey != _adminKey)
            return Unauthorized(new { Success = false, Message = "Invalid admin key." });

        var buyers = await _buyersService.GetAllAsync();
        var users = await _userRepo.GetAllAsync();
        var userCountByBuyer = users.GroupBy(u => u.BuyerId).ToDictionary(g => g.Key, g => g.Count());

        if (onlyWithUsers)
        {
            buyers = buyers.Where(b => userCountByBuyer.ContainsKey(b.Id)).ToList();
        }

        return Ok(new
        {
            Success = true,
            Buyers = buyers.Select(b => new
            {
                b.Id,
                b.Name,
                RegisteredUsers = userCountByBuyer.GetValueOrDefault(b.Id, 0)
            }).ToList()
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBuyer(
        string id,
        [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        if (adminKey != _adminKey)
            return Unauthorized(new { Success = false, Message = "Invalid admin key." });

        var buyers = await _buyersService.GetAllAsync();
        var buyer = buyers.FirstOrDefault(b => b.Id == id);
        if (buyer == null)
            return NotFound(new { Success = false, Message = "Buyer not found." });

        var users = await _userRepo.GetAllAsync();
        var buyerUsers = users
            .Where(u => u.BuyerId == id)
            .Select(u => new
            {
                u.Id,
                u.ContactName,
                u.ContactEmail,
                u.Phone,
                u.CreatedAt,
                u.IsActive
            })
            .ToList();

        return Ok(new
        {
            Success = true,
            Buyer = new
            {
                Id = id,
                buyer.Name,
                Users = buyerUsers
            }
        });
    }
}
