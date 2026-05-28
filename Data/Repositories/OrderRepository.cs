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

    public async Task<bool> HasOverlappingReservationAsync(DateTime startTime, DateTime endTime)
    {
        return await _db.OrderReservations.AnyAsync(r => r.StartTime < endTime && startTime < r.EndTime);
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

    public async Task<List<OrderReservation>> GetFutureReservationsAsync()
    {
        return await _db.OrderReservations
            .Include(r => r.Order)
            .Where(r => r.StartTime >= DateTime.UtcNow && r.Order.ConfirmationStatus != "Cancelled")
            .OrderBy(r => r.StartTime)
            .ToListAsync();
    }
}
