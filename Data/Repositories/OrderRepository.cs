using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasOverlappingReservationAsync(DateOnly day, TimeOnly startTime, TimeOnly endTime)
    {
        return await _db.OrderReservations.AnyAsync(r => r.Day == day && r.StartTime < endTime && startTime < r.EndTime);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _db.Orders.FindAsync(id);
    }

    public async Task UpdateAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task<List<OrderReservation>> GetFutureReservationsAsync(DateOnly? from = null, DateOnly? to = null)
    {
        var query = _db.OrderReservations
            .Include(r => r.Order)
            .Where(r => r.Order.ConfirmationStatus != "Cancelled")
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.Day >= from.Value);
        else
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);
            query = query.Where(r => r.Day > today || (r.Day == today && r.StartTime >= currentTime));
        }

        if (to.HasValue)
            query = query.Where(r => r.Day <= to.Value);

        return await query
            .OrderBy(r => r.Day)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }
}
