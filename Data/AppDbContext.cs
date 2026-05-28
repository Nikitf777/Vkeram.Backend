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
    public DbSet<OrderDelivery> OrderDeliveries => Set<OrderDelivery>();
    public DbSet<ProductReservation> ProductReservations => Set<ProductReservation>();

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
            entity.Property(e => e.ConfirmationStatus).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.ShipmentStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderReservation>(entity =>
        {
            entity.HasIndex(e => new { e.StartTime, e.EndTime });
        });

        modelBuilder.Entity<OrderDelivery>(entity =>
        {
            entity.HasIndex(e => e.OrderId);
        });

        modelBuilder.Entity<ProductReservation>(entity =>
        {
            entity.HasIndex(e => new { e.OrderReservationId, e.ProductId }).IsUnique();
            entity.HasIndex(e => new { e.OrderDeliveryId, e.ProductId }).IsUnique();
        });
    }
}
