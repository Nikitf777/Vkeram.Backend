using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IReservationDurationRepository
{
    Task<ReservationDuration?> GetAsync();
    Task UpdateAsync(ReservationDuration reservationDuration);
}
