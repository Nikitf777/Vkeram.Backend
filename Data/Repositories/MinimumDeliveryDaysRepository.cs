using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class MinimumDeliveryDaysRepository : IMinimumDeliveryDaysRepository
{
    private readonly AppDbContext _db;

    public MinimumDeliveryDaysRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MinimumDeliveryDays?> GetAsync()
    {
        return await _db.MinimumDeliveryDays.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MinimumDeliveryDays minimumDeliveryDays)
    {
        _db.MinimumDeliveryDays.Update(minimumDeliveryDays);
        await _db.SaveChangesAsync();
    }
}
