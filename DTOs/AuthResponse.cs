namespace Vkeram.Backend.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? BuyerId { get; set; }
    public string? Token { get; set; }
}
