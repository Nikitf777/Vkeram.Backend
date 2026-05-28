using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IMinimumBookingDaysRepository
{
    Task<MinimumBookingDays?> GetAsync();
    Task UpdateAsync(MinimumBookingDays minimumBookingDays);
}
