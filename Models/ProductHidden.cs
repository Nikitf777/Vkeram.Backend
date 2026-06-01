using System.ComponentModel.DataAnnotations;

namespace Vkeram.Backend.Models;

public class ProductHidden
{
    [Key, MaxLength(100)]
    public string ProductId { get; set; } = string.Empty;

    public bool IsHidden { get; set; }
}
