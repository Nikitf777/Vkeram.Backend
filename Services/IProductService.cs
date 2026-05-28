namespace Vkeram.Backend.Services;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IProductService
{
    Task<Dictionary<int, ProductDto>> GetByIdsAsync(IEnumerable<int> ids);
}
