using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class AllowDeliveryRepository : IAllowDeliveryRepository
{
    private readonly AppDbContext _db;

    public AllowDeliveryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AllowDelivery?> GetAsync()
    {
        return await _db.AllowDelivery.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AllowDelivery allowDelivery)
    {
        _db.AllowDelivery.Update(allowDelivery);
        await _db.SaveChangesAsync();
    }
}
