using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class DefaultBreakRepository : IDefaultBreakRepository
{
    private readonly AppDbContext _db;

    public DefaultBreakRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DefaultBreak>> GetAllAsync()
    {
        return await _db.DefaultBreaks.OrderBy(b => b.StartTime).ToListAsync();
    }

    public async Task<DefaultBreak?> GetByIdAsync(int id)
    {
        return await _db.DefaultBreaks.FindAsync(id);
    }

    public async Task<DefaultBreak> CreateAsync(DefaultBreak breakItem)
    {
        _db.DefaultBreaks.Add(breakItem);
        await _db.SaveChangesAsync();
        return breakItem;
    }

    public async Task UpdateAsync(DefaultBreak breakItem)
    {
        _db.DefaultBreaks.Update(breakItem);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(DefaultBreak breakItem)
    {
        _db.DefaultBreaks.Remove(breakItem);
        await _db.SaveChangesAsync();
    }
}
