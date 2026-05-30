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

    public async Task<bool> HasOverlappingReservationAsync(DateOnly day, TimeOnly startTime, TimeOnly endTime, int bufferMinutes = 0)
    {
        var bufferedStart = startTime.AddMinutes(-bufferMinutes);
        var bufferedEnd = endTime.AddMinutes(bufferMinutes);
        return await _db.OrderReservations.AnyAsync(r => r.Day == day && r.StartTime < bufferedEnd && bufferedStart < r.EndTime);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _db.Orders
            .Include(o => o.Reservations)
                .ThenInclude(r => r.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Include(o => o.Deliveries)
                .ThenInclude(d => d.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .FirstOrDefaultAsync(o => o.Id == id);
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

    public async Task<List<Order>> GetAllAsync()
    {
        return await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Reservations)
                .ThenInclude(r => r.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Include(o => o.Deliveries)
                .ThenInclude(d => d.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByUserIdAsync(int userId)
    {
        return await _db.Orders
            .Include(o => o.Reservations)
                .ThenInclude(r => r.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Include(o => o.Deliveries)
                .ThenInclude(d => d.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderReservation?> GetReservationByIdAsync(int id)
    {
        return await _db.OrderReservations.FindAsync(id);
    }

    public async Task<OrderDelivery?> GetDeliveryByIdAsync(int id)
    {
        return await _db.OrderDeliveries.FindAsync(id);
    }

    public async Task UpdateReservationAsync(OrderReservation reservation)
    {
        _db.OrderReservations.Update(reservation);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateDeliveryAsync(OrderDelivery delivery)
    {
        _db.OrderDeliveries.Update(delivery);
        await _db.SaveChangesAsync();
    }
}
