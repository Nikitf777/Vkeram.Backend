using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class WorkingHoursRepository : IWorkingHoursRepository
{
    private readonly AppDbContext _db;

    public WorkingHoursRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WorkingHours?> GetAsync()
    {
        return await _db.WorkingHours.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(WorkingHours workingHours)
    {
        _db.WorkingHours.Update(workingHours);
        await _db.SaveChangesAsync();
    }
}
