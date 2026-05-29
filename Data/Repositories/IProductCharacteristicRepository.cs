using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IProductCharacteristicRepository
{
    Task<ProductCharacteristic?> GetByProductIdAsync(string productId);
    Task<ProductCharacteristic> CreateAsync(ProductCharacteristic characteristic);
    Task<ProductCharacteristic> UpdateAsync(ProductCharacteristic characteristic);
    Task<bool> DeleteByProductIdAsync(string productId);
}
