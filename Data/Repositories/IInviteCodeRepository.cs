using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public interface IInviteCodeRepository
{
    Task<InviteCode?> GetByCodeAsync(string code);
    Task MarkAsUsedAsync(InviteCode invite, int userId);
    Task CreateRangeAsync(IEnumerable<InviteCode> invites);
    Task<List<InviteCode>> GetAllAsync();
    Task RevokeRangeAsync(List<int> ids);
}
