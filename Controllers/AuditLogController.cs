using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.DTOs;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditLogController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? action,
        [FromQuery] int? userId,
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId.Value);
        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details,
                UserId = a.UserId,
                UserName = a.UserName,
                UserRole = a.UserRole,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp,
            })
            .ToListAsync();

        return Ok(new
        {
            data = logs,
            total,
            page,
            pageSize,
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _db.AuditLogs.FindAsync(id);
        if (log == null)
            return NotFound(new { message = "Audit log not found." });

        return Ok(new AuditLogDto
        {
            Id = log.Id,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Details = log.Details,
            UserId = log.UserId,
            UserName = log.UserName,
            UserRole = log.UserRole,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp,
        });
    }
}
