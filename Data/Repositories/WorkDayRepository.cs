using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class WorkDayRepository : IWorkDayRepository
{
    private readonly AppDbContext _db;

    public WorkDayRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkDay>> GetAllAsync()
    {
        return await _db.WorkDays.OrderBy(w => w.Id).ToListAsync();
    }

    public async Task<WorkDay?> GetByIdAsync(int id)
    {
        return await _db.WorkDays.FindAsync(id);
    }

    public async Task UpdateAsync(WorkDay workDay)
    {
        _db.WorkDays.Update(workDay);
        await _db.SaveChangesAsync();
    }
}
