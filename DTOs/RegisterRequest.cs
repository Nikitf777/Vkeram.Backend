using System.ComponentModel.DataAnnotations;

namespace InviteOnlyECommerse.DTOs;

public class RegisterRequest
{
    [Required]
    public string InviteCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }
}
