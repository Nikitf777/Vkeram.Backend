using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IWorkingHoursRepository
{
    Task<WorkingHours?> GetAsync();
    Task UpdateAsync(WorkingHours workingHours);
}
