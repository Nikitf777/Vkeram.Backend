namespace Vkeram.Backend.Services;

public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public interface IProductService
{
    Task<Dictionary<string, ProductDto>> GetByIdsAsync(IEnumerable<string> ids);
    Task<List<ProductDto>> GetAllAsync();
}
