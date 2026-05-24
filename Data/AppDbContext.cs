using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Models;

namespace PharmacyWmsBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ThresholdSettings> ThresholdSettings => Set<ThresholdSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<ExpiryChangeRequest> ExpiryChangeRequests => Set<ExpiryChangeRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.MaterialSku).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<ThresholdSettings>(entity =>
        {
            entity.HasIndex(t => t.Key).IsUnique();
        });

        // Seed default thresholds
        modelBuilder.Entity<ThresholdSettings>().HasData(
            new ThresholdSettings { Id = 1, Key = "low_stock_threshold", Value = 100 },
            new ThresholdSettings { Id = 2, Key = "expiring_soon_days", Value = 30 }
        );
    }
}
