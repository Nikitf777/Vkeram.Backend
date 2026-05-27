namespace InviteOnlyECommerse.DTOs;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? CompanyName { get; set; }
}
