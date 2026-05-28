using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IWorkDayRepository
{
    Task<List<WorkDay>> GetAllAsync();
    Task<WorkDay?> GetByIdAsync(int id);
    Task UpdateAsync(WorkDay workDay);
}
