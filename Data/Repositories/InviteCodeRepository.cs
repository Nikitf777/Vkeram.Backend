using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class InviteCodeRepository : IInviteCodeRepository
{
    private readonly AppDbContext _db;

    public InviteCodeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<InviteCode?> GetByCodeAsync(string code)
    {
        return await _db.InviteCodes.FirstOrDefaultAsync(i => i.Code == code && !i.IsUsed && !i.IsRevoked);
    }

    public async Task RevokeRangeAsync(List<int> ids)
    {
        var invites = await _db.InviteCodes.Where(i => ids.Contains(i.Id) && !i.IsUsed && !i.IsRevoked).ToListAsync();
        foreach (var invite in invites)
        {
            invite.IsRevoked = true;
        }
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsUsedAsync(InviteCode invite, int userId)
    {
        invite.IsUsed = true;
        invite.UsedByUserId = userId;
        invite.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task CreateRangeAsync(IEnumerable<InviteCode> invites)
    {
        _db.InviteCodes.AddRange(invites);
        await _db.SaveChangesAsync();
    }

    public async Task<List<InviteCode>> GetAllAsync()
    {
        return await _db.InviteCodes.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }
}
