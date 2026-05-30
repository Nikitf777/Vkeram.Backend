using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductImagePreviewRepository : IProductImagePreviewRepository
{
    private readonly AppDbContext _db;

    public ProductImagePreviewRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ProductImagePreview preview)
    {
        _db.ProductImagePreviews.Add(preview);
        await _db.SaveChangesAsync();
    }

    public async Task<ProductImagePreview?> GetByIdAsync(int id)
    {
        return await _db.ProductImagePreviews.FindAsync(id);
    }

    public async Task<ProductImagePreview?> GetByImageIdAsync(int imageId)
    {
        return await _db.ProductImagePreviews.FirstOrDefaultAsync(p => p.ImageId == imageId);
    }

    public async Task<List<ProductImagePreview>> GetByProductIdAsync(string productId)
    {
        return await _db.ProductImagePreviews
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(ProductImagePreview preview)
    {
        _db.ProductImagePreviews.Remove(preview);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByImageIdAsync(int imageId)
    {
        await _db.ProductImagePreviews.Where(p => p.ImageId == imageId).ExecuteDeleteAsync();
    }
}
