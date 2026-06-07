using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IOrderRepository
{
    Task<bool> HasOverlappingReservationAsync(DateOnly day, TimeOnly startTime, TimeOnly endTime, int bufferMinutes = 0);
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task UpdateAsync(Order order);
    Task<List<OrderReservation>> GetFutureReservationsAsync(DateOnly? from = null, DateOnly? to = null);
    Task<List<Order>> GetAllAsync(DateTime? from = null, DateTime? to = null, bool? isConfirmed = null, string? paymentStatus = null, string? shipmentStatus = null, string? buyerId = null, int? userId = null);
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<List<Order>> GetByUserIdsAsync(List<int> userIds);
    Task<OrderReservation?> GetReservationByIdAsync(int id);
    Task<OrderDelivery?> GetDeliveryByIdAsync(int id);
    Task UpdateReservationAsync(OrderReservation reservation);
    Task UpdateDeliveryAsync(OrderDelivery delivery);
    Task<List<OrderReservation>> GetReservationsAsync(DateOnly? minDate = null, DateOnly? maxDate = null);
    Task<List<OrderDelivery>> GetDeliveriesAsync(DateTime? minDate = null, DateTime? maxDate = null);
}
