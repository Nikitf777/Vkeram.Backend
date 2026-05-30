namespace Vkeram.Backend.Models;

public class ProductImagePreview
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int ImageId { get; set; }
    public byte[] ImageData { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
