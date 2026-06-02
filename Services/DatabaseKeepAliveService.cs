using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;

namespace PharmacyWmsBackend.Services;

public class DatabaseKeepAliveService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseKeepAliveService> _logger;

    public DatabaseKeepAliveService(IServiceScopeFactory scopeFactory, ILogger<DatabaseKeepAliveService> logger)
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
                await db.Database.ExecuteSqlRawAsync("SELECT 1", stoppingToken);
                _logger.LogDebug("Database keep-alive ping sent.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database keep-alive ping failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken);
        }
    }
}
