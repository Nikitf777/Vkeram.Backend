using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IAllowDeliveryRepository
{
    Task<AllowDelivery?> GetAsync();
    Task UpdateAsync(AllowDelivery allowDelivery);
}
