using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class ReservationDurationRepository : IReservationDurationRepository
{
    private readonly AppDbContext _db;

    public ReservationDurationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReservationDuration?> GetAsync()
    {
        return await _db.ReservationDuration.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(ReservationDuration reservationDuration)
    {
        _db.ReservationDuration.Update(reservationDuration);
        await _db.SaveChangesAsync();
    }
}
