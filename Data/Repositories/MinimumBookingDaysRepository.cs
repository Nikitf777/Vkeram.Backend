using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class MinimumBookingDaysRepository : IMinimumBookingDaysRepository
{
    private readonly AppDbContext _db;

    public MinimumBookingDaysRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MinimumBookingDays?> GetAsync()
    {
        return await _db.MinimumBookingDays.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MinimumBookingDays minimumBookingDays)
    {
        _db.MinimumBookingDays.Update(minimumBookingDays);
        await _db.SaveChangesAsync();
    }
}
