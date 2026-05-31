using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class CreateInviteRequest
{
    [MaxLength(200)]
    public string? BuyerId { get; set; }

    public int ExpiresInDays { get; set; } = 30;

    public int Count { get; set; } = 1;
}
