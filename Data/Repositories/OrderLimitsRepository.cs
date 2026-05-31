using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class OrderLimitsRepository : IOrderLimitsRepository
{
    private readonly AppDbContext _db;

    public OrderLimitsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderLimits?> GetAsync()
    {
        return await _db.OrderLimits.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(OrderLimits orderLimits)
    {
        _db.OrderLimits.Update(orderLimits);
        await _db.SaveChangesAsync();
    }
}
