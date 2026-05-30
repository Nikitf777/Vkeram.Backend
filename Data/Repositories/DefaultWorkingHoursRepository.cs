using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data.Repositories;

public class DefaultWorkingHoursRepository : IDefaultWorkingHoursRepository
{
    private readonly AppDbContext _db;

    public DefaultWorkingHoursRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DefaultWorkingHours?> GetAsync()
    {
        return await _db.DefaultWorkingHours.FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(DefaultWorkingHours workingHours)
    {
        _db.DefaultWorkingHours.Update(workingHours);
        await _db.SaveChangesAsync();
    }
}
