namespace Vkeram.Backend.DTOs;

public class InviteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Codes { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
}
