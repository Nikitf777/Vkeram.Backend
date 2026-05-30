using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductImagePreviewRepository
{
    Task AddAsync(ProductImagePreview preview);
    Task<ProductImagePreview?> GetByIdAsync(int id);
    Task<ProductImagePreview?> GetByImageIdAsync(int imageId);
    Task<List<ProductImagePreview>> GetByProductIdAsync(string productId);
    Task DeleteAsync(ProductImagePreview preview);
    Task DeleteByImageIdAsync(int imageId);
}
