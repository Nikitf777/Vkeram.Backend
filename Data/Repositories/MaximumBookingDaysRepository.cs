using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class MaximumBookingDaysRepository : IMaximumBookingDaysRepository
{
    private readonly AppDbContext _db;

    public MaximumBookingDaysRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MaximumBookingDays?> GetAsync()
    {
        return await _db.MaximumBookingDays.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MaximumBookingDays maximumBookingDays)
    {
        _db.MaximumBookingDays.Update(maximumBookingDays);
        await _db.SaveChangesAsync();
    }
}
