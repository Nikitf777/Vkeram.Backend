using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class AllowBookingRepository : IAllowBookingRepository
{
    private readonly AppDbContext _db;

    public AllowBookingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AllowBooking?> GetAsync()
    {
        return await _db.AllowBooking.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AllowBooking allowBooking)
    {
        _db.AllowBooking.Update(allowBooking);
        await _db.SaveChangesAsync();
    }
}
