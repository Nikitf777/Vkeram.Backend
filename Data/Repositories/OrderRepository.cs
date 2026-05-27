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
}
