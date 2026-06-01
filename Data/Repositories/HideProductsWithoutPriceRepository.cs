using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class HideProductsWithoutPriceRepository : IHideProductsWithoutPriceRepository
{
    private readonly AppDbContext _db;

    public HideProductsWithoutPriceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HideProductsWithoutPrice?> GetAsync()
    {
        return await _db.HideProductsWithoutPrice.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(HideProductsWithoutPrice setting)
    {
        _db.HideProductsWithoutPrice.Update(setting);
        await _db.SaveChangesAsync();
    }
}
