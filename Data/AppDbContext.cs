using Microsoft.EntityFrameworkCore;
using InviteOnlyECommerse.Models;

namespace InviteOnlyECommerse.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.ContactEmail).IsUnique();
        });

        modelBuilder.Entity<InviteCode>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsUsed);
        });
    }
}
