using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IDefaultWorkingHoursRepository
{
    Task<DefaultWorkingHours?> GetAsync();
    Task UpdateAsync(DefaultWorkingHours workingHours);
}
