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
}
