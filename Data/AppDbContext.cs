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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.MaterialSku).IsUnique();
            // Explicit timestamp type to avoid Npgsql DateTimeKind mismatch
            entity.Property(p => p.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<ThresholdSettings>(entity =>
        {
            entity.HasIndex(t => t.Key).IsUnique();
        });

        // Explicit timestamp type for AuditLog — fixes 500 on Supabase/Npgsql strict mode
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(a => a.Timestamp)
                  .HasColumnType("timestamp with time zone");
        });

        // Explicit timestamp types for Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CreatedAt).HasColumnType("timestamp with time zone");
        });

        // Explicit timestamp types for Notifications
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(c => c.CreatedAt).HasColumnType("timestamp with time zone");
        });

        // Seed default thresholds
        modelBuilder.Entity<ThresholdSettings>().HasData(
            new ThresholdSettings { Id = 1, Key = "low_stock_threshold", Value = 100 },
            new ThresholdSettings { Id = 2, Key = "expiring_soon_days", Value = 30 }
        );
    }
}
