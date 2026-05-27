using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Name = "Concrete Block", Description = "Standard 8x8x16 concrete block", UnitPrice = 1.50m },
                new Product { Name = "Cement Bag", Description = "50kg Portland cement", UnitPrice = 12.00m },
                new Product { Name = "Steel Rebar", Description = "12mm x 6m steel reinforcement bar", UnitPrice = 8.50m },
                new Product { Name = "Sand", Description = "Fine construction sand per ton", UnitPrice = 35.00m },
                new Product { Name = "Gravel", Description = "Crushed stone gravel per ton", UnitPrice = 40.00m }
            );

            await db.SaveChangesAsync();
        }

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
    }
}
