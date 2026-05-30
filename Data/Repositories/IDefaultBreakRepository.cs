using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IDefaultBreakRepository
{
    Task<List<DefaultBreak>> GetAllAsync();
    Task<DefaultBreak?> GetByIdAsync(int id);
    Task<DefaultBreak> CreateAsync(DefaultBreak breakItem);
    Task UpdateAsync(DefaultBreak breakItem);
    Task DeleteAsync(DefaultBreak breakItem);
}
