using Microsoft.EntityFrameworkCore;
using Vkeram.Backend.Models;

namespace Vkeram.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderReservation> OrderReservations => Set<OrderReservation>();

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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderReservation>(entity =>
        {
            entity.HasIndex(e => new { e.StartTime, e.EndTime });
        });
    }
}
