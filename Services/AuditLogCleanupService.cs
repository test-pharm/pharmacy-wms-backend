using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;

namespace PharmacyWmsBackend.Services;

public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogCleanupService> _logger;

    public AuditLogCleanupService(IServiceScopeFactory scopeFactory, ILogger<AuditLogCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-30);
                var deleted = await db.AuditLogs
                    .Where(log => log.Timestamp < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    _logger.LogInformation("Deleted {Count} audit log(s) older than 30 days.", deleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit log cleanup failed.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
