using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class InviteCode
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    public bool IsUsed { get; set; } = false;

    public int? UsedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UsedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
