using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductImageRepository : IProductImageRepository
{
    private readonly AppDbContext _db;

    public ProductImageRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ProductImage image)
    {
        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync();
    }

    public async Task<ProductImage?> GetByIdAsync(int id)
    {
        return await _db.ProductImages.FindAsync(id);
    }

    public async Task<List<ProductImage>> GetByProductIdAsync(string productId)
    {
        return await _db.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(ProductImage image)
    {
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();
    }
}
