using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IMaximumDeliveryDaysRepository
{
    Task<MaximumDeliveryDays?> GetAsync();
    Task UpdateAsync(MaximumDeliveryDays maximumDeliveryDays);
}
