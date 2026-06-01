using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.DTOs;

public class CreateInviteRequest
{
    [Required, MaxLength(200)]
    public string BuyerId { get; set; } = string.Empty;

    public int ExpiresInDays { get; set; } = 30;

    public int Count { get; set; } = 1;
}
