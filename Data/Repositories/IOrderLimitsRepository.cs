using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IOrderLimitsRepository
{
    Task<OrderLimits?> GetAsync();
    Task UpdateAsync(OrderLimits orderLimits);
}
