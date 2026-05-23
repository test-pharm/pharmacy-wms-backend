using System.Security.Claims;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.Models;

namespace PharmacyWmsBackend.Services;

public class AuditLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditLogService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(string action, string entityType, int? entityId, string? details = null)
    {
        var principal = _http.HttpContext?.User;
        var userId = 0;
        var userName = "System";
        var userRole = "";
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        if (principal?.Identity?.IsAuthenticated == true)
        {
            int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
            userName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            userRole = principal.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            IpAddress = ip,
            Timestamp = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
    }
}
