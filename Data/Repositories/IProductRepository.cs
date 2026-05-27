using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductRepository
{
    Task<Dictionary<int, Product>> GetByIdsAsync(IEnumerable<int> ids);
}
