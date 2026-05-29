namespace Vkeram.Backend.Models;

public class ProductImage
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
