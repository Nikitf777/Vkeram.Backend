using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IOrderRepository
{
    Task<bool> HasOverlappingReservationAsync(DateOnly day, TimeOnly startTime, TimeOnly endTime);
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task UpdateAsync(Order order);
    Task<List<OrderReservation>> GetFutureReservationsAsync(DateOnly? from = null, DateOnly? to = null);
}
