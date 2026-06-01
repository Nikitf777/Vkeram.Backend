using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductHiddenRepository : IProductHiddenRepository
{
    private readonly AppDbContext _db;

    public ProductHiddenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProductHidden?> GetByProductIdAsync(string productId)
    {
        return await _db.ProductHidden.FindAsync(productId);
    }

    public async Task<List<ProductHidden>> GetAllAsync()
    {
        return await _db.ProductHidden.ToListAsync();
    }

    public async Task SetHiddenAsync(string productId, bool isHidden)
    {
        var existing = await _db.ProductHidden.FindAsync(productId);
        if (existing != null)
        {
            existing.IsHidden = isHidden;
        }
        else
        {
            _db.ProductHidden.Add(new ProductHidden { ProductId = productId, IsHidden = isHidden });
        }
        await _db.SaveChangesAsync();
    }
}
