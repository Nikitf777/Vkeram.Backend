using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IMinimumDeliveryDaysRepository
{
    Task<MinimumDeliveryDays?> GetAsync();
    Task UpdateAsync(MinimumDeliveryDays minimumDeliveryDays);
}
