using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductPriceRepository : IProductPriceRepository
{
    private readonly AppDbContext _db;

    public ProductPriceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ProductPrice price)
    {
        _db.ProductPrices.Add(price);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ProductPrice>> GetLatestPerProductAsync()
    {
        var allPrices = await _db.ProductPrices.ToListAsync();
        return allPrices
            .GroupBy(p => p.ProductId)
            .Select(g => g.OrderByDescending(p => p.CreatedAt).First())
            .ToList();
    }

    public async Task<ProductPrice?> GetLatestForProductAsync(string productId)
    {
        return await _db.ProductPrices
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProductPrice>> GetHistoryForProductAsync(string productId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.ProductPrices
            .Where(p => p.ProductId == productId);

        if (from.HasValue)
            query = query.Where(p => p.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.CreatedAt <= to.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
