using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductHiddenRepository
{
    Task<ProductHidden?> GetByProductIdAsync(string productId);
    Task<List<ProductHidden>> GetAllAsync();
    Task SetHiddenAsync(string productId, bool isHidden);
}
