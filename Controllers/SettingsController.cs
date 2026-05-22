using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.DTOs;
using PharmacyWmsBackend.Models;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet("thresholds")]
    public async Task<IActionResult> GetThresholds()
    {
        var settings = await _db.ThresholdSettings.ToListAsync();
        return Ok(new ThresholdResponse
        {
            LowStockThreshold = settings.FirstOrDefault(s => s.Key == "low_stock_threshold")?.Value ?? 100,
            ExpiringSoonDays = settings.FirstOrDefault(s => s.Key == "expiring_soon_days")?.Value ?? 30,
        });
    }

    [HttpPut("thresholds")]
    public async Task<IActionResult> UpdateThresholds([FromBody] UpdateThresholdRequest request)
    {
        if (request.LowStockThreshold.HasValue)
        {
            var setting = await _db.ThresholdSettings
                .FirstOrDefaultAsync(s => s.Key == "low_stock_threshold");
            if (setting != null)
            {
                setting.Value = request.LowStockThreshold.Value;
            }
            else
            {
                _db.ThresholdSettings.Add(new ThresholdSettings
                {
                    Key = "low_stock_threshold",
                    Value = request.LowStockThreshold.Value,
                });
            }
        }

        if (request.ExpiringSoonDays.HasValue)
        {
            var setting = await _db.ThresholdSettings
                .FirstOrDefaultAsync(s => s.Key == "expiring_soon_days");
            if (setting != null)
            {
                setting.Value = request.ExpiringSoonDays.Value;
            }
            else
            {
                _db.ThresholdSettings.Add(new ThresholdSettings
                {
                    Key = "expiring_soon_days",
                    Value = request.ExpiringSoonDays.Value,
                });
            }
        }

        await _db.SaveChangesAsync();
        return await GetThresholds();
    }
}
