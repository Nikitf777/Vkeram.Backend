using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<int, Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
    }
}
