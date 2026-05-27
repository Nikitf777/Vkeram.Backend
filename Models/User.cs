using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
