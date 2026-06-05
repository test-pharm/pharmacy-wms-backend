using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var threshold = await _db.ThresholdSettings
            .Where(t => t.Key == "low_stock_threshold")
            .Select(t => t.Value)
            .FirstOrDefaultAsync();

        var now = DateTime.UtcNow;

        var products = await _db.Products
            .Include(p => p.Batches)
            .ToListAsync();

        var lowStock = products
            .Where(p => p.Batches.Sum(b => b.Quantity) < threshold)
            .Select(p => new
            {
                Id = p.Id,
                MaterialName = p.MaterialName,
                MaterialSku = p.MaterialSku,
                TotalQuantity = p.Batches.Sum(b => b.Quantity),
                Threshold = threshold,
            })
            .ToList();

        var expiringBatches = products
            .SelectMany(p => p.Batches)
            .Where(b =>
            {
                if (!DateOnly.TryParse(b.ExpiryDate, out var exp)) return false;
                return exp <= DateOnly.FromDateTime(now.AddDays(30)) && exp >= DateOnly.FromDateTime(now);
            })
            .Select(b => new
            {
                BatchId = b.Id,
                ProductId = b.ProductId,
                ProductName = b.Product != null ? b.Product.MaterialName : "",
                ExpiryDate = b.ExpiryDate,
                Quantity = b.Quantity,
                DaysUntilExpiry = b.ExpiryDate != null && DateOnly.TryParse(b.ExpiryDate, out var exp)
                    ? (exp.DayNumber - DateOnly.FromDateTime(now).DayNumber)
                    : 999,
            })
            .OrderBy(b => b.DaysUntilExpiry)
            .ToList();

        return Ok(new { lowStock, expiringBatches });
    }

    [HttpGet("expiry-report")]
    public async Task<IActionResult> GetExpiryReport()
    {
        var now = DateTime.UtcNow;

        var batches = await _db.StockBatches
            .Include(b => b.Product)
            .Where(b => b.Quantity > 0)
            .ToListAsync();

        var report = batches
            .Select(b =>
            {
                DateOnly.TryParse(b.ExpiryDate, out var exp);
                return new
                {
                    BatchId = b.Id,
                    ProductId = b.ProductId,
                    ProductName = b.Product?.MaterialName ?? "",
                    ProductSku = b.Product?.MaterialSku ?? "",
                    ExpiryDate = b.ExpiryDate,
                    Quantity = b.Quantity,
                    Status = exp == default ? "Unknown" :
                             exp < DateOnly.FromDateTime(now) ? "Expired" :
                             exp <= DateOnly.FromDateTime(now.AddDays(30)) ? "Expiring Soon" :
                             "Valid",
                };
            })
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        return Ok(report);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity([FromQuery] int limit = 10)
    {
        var logs = await _db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new {
                a.Id,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Details,
                a.UserName,
                a.UserRole,
                a.Timestamp
            })
            .ToListAsync();
        return Ok(logs);
    }

    [HttpGet("stock-movement")]
    public async Task<IActionResult> GetStockMovement([FromQuery] int days = 30)
    {
        var cutOffDate = DateTime.UtcNow.AddDays(-days);
        var orders = await _db.Orders
            .Where(o => o.CreatedAt >= cutOffDate && o.Status == "completed")
            .ToListAsync();

        var result = orders
            .GroupBy(o => new { Date = o.CreatedAt.ToString("yyyy-MM-dd"), o.Type })
            .Select(g => new {
                Date = g.Key.Date,
                Type = g.Key.Type,
                OrderCount = g.Count(),
                TotalQuantity = g.Sum(o => o.Quantity)
            })
            .OrderBy(r => r.Date)
            .ToList();

        return Ok(result);
    }

    [HttpGet("top-consumed")]
    public async Task<IActionResult> GetTopConsumed([FromQuery] int? month, [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var orders = await _db.Orders
            .Where(o => o.Type == "export" && o.Status == "completed" && o.CreatedAt.Month == targetMonth && o.CreatedAt.Year == targetYear)
            .ToListAsync();

        var result = orders
            .GroupBy(o => new { o.ProductName, o.ProductSku })
            .Select(g => new {
                ProductName = g.Key.ProductName,
                ProductSku = g.Key.ProductSku,
                TotalQuantity = g.Sum(o => o.Quantity)
            })
            .OrderByDescending(r => r.TotalQuantity)
            .Take(5)
            .ToList();

        return Ok(result);
    }
}
