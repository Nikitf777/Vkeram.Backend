using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IAutoConfirmOrdersRepository
{
    Task<AutoConfirmOrders?> GetAsync();
    Task UpdateAsync(AutoConfirmOrders autoConfirmOrders);
}
