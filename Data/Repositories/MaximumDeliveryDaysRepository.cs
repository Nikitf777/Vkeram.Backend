using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class MaximumDeliveryDaysRepository : IMaximumDeliveryDaysRepository
{
    private readonly AppDbContext _db;

    public MaximumDeliveryDaysRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MaximumDeliveryDays?> GetAsync()
    {
        return await _db.MaximumDeliveryDays.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MaximumDeliveryDays maximumDeliveryDays)
    {
        _db.MaximumDeliveryDays.Update(maximumDeliveryDays);
        await _db.SaveChangesAsync();
    }
}
