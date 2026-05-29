using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductCharacteristicRepository : IProductCharacteristicRepository
{
    private readonly AppDbContext _db;

    public ProductCharacteristicRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProductCharacteristic?> GetByProductIdAsync(string productId)
    {
        return await _db.ProductCharacteristics.FirstOrDefaultAsync(c => c.ProductId == productId);
    }

    public async Task<ProductCharacteristic> CreateAsync(ProductCharacteristic characteristic)
    {
        _db.ProductCharacteristics.Add(characteristic);
        await _db.SaveChangesAsync();
        return characteristic;
    }

    public async Task<ProductCharacteristic> UpdateAsync(ProductCharacteristic characteristic)
    {
        _db.ProductCharacteristics.Update(characteristic);
        await _db.SaveChangesAsync();
        return characteristic;
    }

    public async Task<bool> DeleteByProductIdAsync(string productId)
    {
        var entity = await _db.ProductCharacteristics.FirstOrDefaultAsync(c => c.ProductId == productId);
        if (entity == null) return false;
        _db.ProductCharacteristics.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
