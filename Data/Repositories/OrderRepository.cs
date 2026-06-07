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
            .Include(o => o.User)
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
            .Where(r => r.Order.IsConfirmed)
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

    public async Task<List<Order>> GetAllAsync(DateTime? from = null, DateTime? to = null, bool? isConfirmed = null, string? paymentStatus = null, string? shipmentStatus = null, string? buyerId = null, int? userId = null)
    {
        return await FilterOrdersAsync(_db.Orders.AsQueryable(), from, to, isConfirmed, paymentStatus, shipmentStatus, buyerId, userId);
    }

    public async Task<List<Order>> GetByProductIdAsync(string productId, DateTime? from = null, DateTime? to = null, bool? isConfirmed = null, string? paymentStatus = null, string? shipmentStatus = null, string? buyerId = null, int? userId = null)
    {
        var query = _db.Orders
            .Where(o => o.Reservations.Any(r => r.ProductReservations.Any(pr => pr.ProductId == productId))
                     || o.Deliveries.Any(d => d.ProductReservations.Any(pr => pr.ProductId == productId)));
        return await FilterOrdersAsync(query, from, to, isConfirmed, paymentStatus, shipmentStatus, buyerId, userId);
    }

    private async Task<List<Order>> FilterOrdersAsync(IQueryable<Order> query, DateTime? from = null, DateTime? to = null, bool? isConfirmed = null, string? paymentStatus = null, string? shipmentStatus = null, string? buyerId = null, int? userId = null)
    {
        query = query
            .Include(o => o.User)
            .Include(o => o.Reservations)
                .ThenInclude(r => r.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Include(o => o.Deliveries)
                .ThenInclude(d => d.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(o => o.CreatedAt <= to.Value);
        if (isConfirmed.HasValue)
            query = query.Where(o => o.IsConfirmed == isConfirmed.Value);
        if (!string.IsNullOrWhiteSpace(buyerId))
            query = query.Where(o => o.User.BuyerId == buyerId);
        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(paymentStatus))
            orders = orders.Where(o => o.PaymentStatus == paymentStatus).ToList();
        if (!string.IsNullOrWhiteSpace(shipmentStatus))
            orders = orders.Where(o => o.ShipmentStatus == shipmentStatus).ToList();

        return orders;
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

    public async Task<List<Order>> GetByUserIdsAsync(List<int> userIds)
    {
        return await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Reservations)
                .ThenInclude(r => r.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Include(o => o.Deliveries)
                .ThenInclude(d => d.ProductReservations)
                .ThenInclude(pr => pr.ProductPrice)
            .Where(o => userIds.Contains(o.UserId))
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

    public async Task<List<OrderReservation>> GetReservationsAsync(DateOnly? minDate = null, DateOnly? maxDate = null)
    {
        var query = _db.OrderReservations
            .Include(r => r.Order)
            .AsQueryable();

        if (minDate.HasValue)
            query = query.Where(r => r.Day >= minDate.Value);

        if (maxDate.HasValue)
            query = query.Where(r => r.Day <= maxDate.Value);

        return await query
            .OrderBy(r => r.Day)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<List<OrderDelivery>> GetDeliveriesAsync(DateTime? minDate = null, DateTime? maxDate = null)
    {
        var query = _db.OrderDeliveries
            .Include(d => d.Order)
            .AsQueryable();

        if (minDate.HasValue)
            query = query.Where(d => d.DeliveryTime >= minDate.Value);

        if (maxDate.HasValue)
            query = query.Where(d => d.DeliveryTime <= maxDate.Value);

        return await query
            .OrderBy(d => d.DeliveryTime)
            .ToListAsync();
    }
}
