using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!db.InviteCodes.Any())
        {
            db.InviteCodes.AddRange(
                new InviteCode
                {
                    Code = "B2B-INVITE-2024-001",
                    CompanyName = "Acme Corp",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1)
                },
                new InviteCode
                {
                    Code = "B2B-INVITE-2024-002",
                    CompanyName = "Globex Inc",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1)
                },
                new InviteCode
                {
                    Code = "B2B-INVITE-2024-003",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1)
                }
            );

            await db.SaveChangesAsync();
        }

        if (!db.WorkDays.Any())
        {
            db.WorkDays.AddRange(
                new WorkDay { DayName = "Monday", IsWorkingDay = true },
                new WorkDay { DayName = "Tuesday", IsWorkingDay = true },
                new WorkDay { DayName = "Wednesday", IsWorkingDay = true },
                new WorkDay { DayName = "Thursday", IsWorkingDay = true },
                new WorkDay { DayName = "Friday", IsWorkingDay = true },
                new WorkDay { DayName = "Saturday", IsWorkingDay = false },
                new WorkDay { DayName = "Sunday", IsWorkingDay = false }
            );

            await db.SaveChangesAsync();
        }
    }
}
