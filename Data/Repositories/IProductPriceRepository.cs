using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductPriceRepository
{
    Task AddAsync(ProductPrice price);
    Task<List<ProductPrice>> GetLatestPerProductAsync();
    Task<ProductPrice?> GetLatestForProductAsync(string productId);
    Task<List<ProductPrice>> GetHistoryForProductAsync(string productId);
}
