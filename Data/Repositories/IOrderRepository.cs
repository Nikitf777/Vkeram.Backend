using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IOrderRepository
{
    Task<bool> HasOverlappingReservationAsync(DateTime startTime, DateTime endTime);
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task UpdateAsync(Order order);
}
