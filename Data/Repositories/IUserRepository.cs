using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<List<User>> GetAllAsync();
}
