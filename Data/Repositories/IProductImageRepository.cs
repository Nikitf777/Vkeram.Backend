using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductImageRepository
{
    Task AddAsync(ProductImage image);
    Task<ProductImage?> GetByIdAsync(int id);
    Task<List<ProductImage>> GetByProductIdAsync(string productId);
    Task DeleteAsync(ProductImage image);
}
