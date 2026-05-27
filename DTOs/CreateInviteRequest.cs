using System.ComponentModel.DataAnnotations;

namespace InviteOnlyECommerse.DTOs;

public class CreateInviteRequest
{
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    public int ExpiresInDays { get; set; } = 30;

    public int Count { get; set; } = 1;
}
