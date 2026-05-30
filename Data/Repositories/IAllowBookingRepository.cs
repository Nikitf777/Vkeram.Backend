using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IAllowBookingRepository
{
    Task<AllowBooking?> GetAsync();
    Task UpdateAsync(AllowBooking allowBooking);
}
