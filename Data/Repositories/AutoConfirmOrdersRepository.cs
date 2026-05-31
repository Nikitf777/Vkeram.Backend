using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class AutoConfirmOrdersRepository : IAutoConfirmOrdersRepository
{
    private readonly AppDbContext _db;

    public AutoConfirmOrdersRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AutoConfirmOrders?> GetAsync()
    {
        return await _db.AutoConfirmOrders.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AutoConfirmOrders autoConfirmOrders)
    {
        _db.AutoConfirmOrders.Update(autoConfirmOrders);
        await _db.SaveChangesAsync();
    }
}
