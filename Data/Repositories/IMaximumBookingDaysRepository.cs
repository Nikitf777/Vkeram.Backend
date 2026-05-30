using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IMaximumBookingDaysRepository
{
    Task<MaximumBookingDays?> GetAsync();
    Task UpdateAsync(MaximumBookingDays maximumBookingDays);
}
