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
    public DbSet<WorkDay> WorkDays => Set<WorkDay>();
    public DbSet<DefaultWorkingHours> DefaultWorkingHours => Set<DefaultWorkingHours>();
    public DbSet<DefaultBreak> DefaultBreaks => Set<DefaultBreak>();
    public DbSet<MinimumBookingDays> MinimumBookingDays => Set<MinimumBookingDays>();
    public DbSet<MaximumBookingDays> MaximumBookingDays => Set<MaximumBookingDays>();
    public DbSet<MinimumDeliveryDays> MinimumDeliveryDays => Set<MinimumDeliveryDays>();
    public DbSet<MaximumDeliveryDays> MaximumDeliveryDays => Set<MaximumDeliveryDays>();
    public DbSet<AllowBooking> AllowBooking => Set<AllowBooking>();
    public DbSet<AllowDelivery> AllowDelivery => Set<AllowDelivery>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductImagePreview> ProductImagePreviews => Set<ProductImagePreview>();
    public DbSet<ProductCharacteristic> ProductCharacteristics => Set<ProductCharacteristic>();
    public DbSet<ReservationDuration> ReservationDuration => Set<ReservationDuration>();
    public DbSet<OrderLimits> OrderLimits => Set<OrderLimits>();
    public DbSet<AutoConfirmOrders> AutoConfirmOrders => Set<AutoConfirmOrders>();

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
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<OrderReservation>(entity =>
        {
            entity.HasIndex(e => new { e.Day, e.StartTime, e.EndTime });
        });

        modelBuilder.Entity<OrderDelivery>(entity =>
        {
            entity.HasIndex(e => e.OrderId);
        });

        modelBuilder.Entity<ProductReservation>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(100);
            entity.HasIndex(e => new { e.OrderReservationId, e.ProductId }).IsUnique();
            entity.HasIndex(e => new { e.OrderDeliveryId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.ProductPrice).WithMany().HasForeignKey(e => e.ProductPriceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkDay>(entity =>
        {
            entity.HasIndex(e => e.DayName).IsUnique();
            entity.Property(e => e.DayName).HasMaxLength(20);
        });

        modelBuilder.Entity<DefaultWorkingHours>(entity =>
        {
            entity.HasData(new DefaultWorkingHours { Id = 1, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) });
        });

        modelBuilder.Entity<DefaultBreak>(entity =>
        {
        });

        modelBuilder.Entity<MinimumBookingDays>(entity =>
        {
            entity.HasData(new MinimumBookingDays { Id = 1, Days = 1, CountWorkingDaysOnly = false });
        });

        modelBuilder.Entity<MinimumDeliveryDays>(entity =>
        {
            entity.HasData(new MinimumDeliveryDays { Id = 1, Days = 1, CountWorkingDaysOnly = false });
        });

        modelBuilder.Entity<MaximumBookingDays>(entity =>
        {
            entity.HasData(new MaximumBookingDays { Id = 1, Days = 30, CountWorkingDaysOnly = false });
        });

        modelBuilder.Entity<MaximumDeliveryDays>(entity =>
        {
            entity.HasData(new MaximumDeliveryDays { Id = 1, Days = 30, CountWorkingDaysOnly = false });
        });

        modelBuilder.Entity<AllowBooking>(entity =>
        {
            entity.HasData(new AllowBooking { Id = 1, IsAllowed = true });
        });

        modelBuilder.Entity<AllowDelivery>(entity =>
        {
            entity.HasData(new AllowDelivery { Id = 1, IsAllowed = true });
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.ProductId, e.CreatedAt });
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(100);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.HasIndex(e => e.ProductId);
        });

        modelBuilder.Entity<ReservationDuration>(entity =>
        {
            entity.HasData(new ReservationDuration { Id = 1, DurationMinutes = 30 });
        });

        modelBuilder.Entity<OrderLimits>(entity =>
        {
            entity.HasData(new OrderLimits
            {
                Id = 1,
                MinOrderPrice = 0,
                MaxOrderPrice = 1000000,
                MinOrderQuantity = 1,
                MaxOrderQuantity = 10000,
                MinReservationQuantity = 1,
                MaxReservationQuantity = 100,
                MinDeliveryQuantity = 1,
                MaxDeliveryQuantity = 100,
                MinProductReservationQuantity = 1,
                MaxProductReservationQuantity = 1000
            });
        });

        modelBuilder.Entity<AutoConfirmOrders>(entity =>
        {
            entity.HasData(new AutoConfirmOrders
            {
                Id = 1,
                IsEnabled = false,
                MaxAutoConfirmPrice = 10000,
                MaxAutoConfirmQuantity = 100
            });
        });

        modelBuilder.Entity<ProductImagePreview>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(100);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ImageId);
        });

        modelBuilder.Entity<ProductCharacteristic>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(100);
            entity.Property(e => e.StrengthGrade).HasMaxLength(100);
            entity.Property(e => e.FrostResistance).HasMaxLength(100);
            entity.Property(e => e.WaterAbsorption).HasMaxLength(100);
            entity.Property(e => e.RadiationQuality).HasMaxLength(100);
            entity.Property(e => e.Standard).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.BrickType).HasMaxLength(100);
            entity.HasIndex(e => e.ProductId).IsUnique();
        });
    }
}
