using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion(
                      v => v.Value,
                      v => OrderStatus.FromString(v));
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            
            entity.HasMany(e => e.OrderItems)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
        });
    }
} 