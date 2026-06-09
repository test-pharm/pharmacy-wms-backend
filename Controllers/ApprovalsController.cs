using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.DTOs;
using PharmacyWmsBackend.Models;
using PharmacyWmsBackend.Services;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public ApprovalsController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _db.Set<ExpiryChangeRequest>()
            .Include(r => r.Batch)
            .ThenInclude(b => b!.Product)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var requests = await _db.Set<ExpiryChangeRequest>()
            .Include(r => r.Batch)
            .ThenInclude(b => b!.Product)
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var username = User.Identity?.Name ?? "";
        var requests = await _db.Set<ExpiryChangeRequest>()
            .Include(r => r.Batch)
            .ThenInclude(b => b!.Product)
            .Where(r => r.RequestedBy == username)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] ExpiryChangeRequestDto dto)
    {
        var batch = await _db.StockBatches.FindAsync(dto.BatchId);
        if (batch == null)
            return NotFound(new { message = "Batch not found." });

        var request = new ExpiryChangeRequest
        {
            BatchId = dto.BatchId,
            OldExpiry = batch.ExpiryDate,
            NewExpiry = dto.NewExpiry,
            Reason = dto.Reason,
            RequestedBy = User.Identity?.Name ?? "unknown",
            RequestedAt = DateTime.UtcNow,
            Status = "Pending",
        };

        _db.Set<ExpiryChangeRequest>().Add(request);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("ExpiryChangeRequest", "StockBatch", batch.Id,
            $"Expiry change requested for batch {batch.Id}: {batch.ExpiryDate} → {dto.NewExpiry}");

        return Ok(request);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewRequestDto dto)
    {
        var request = await _db.Set<ExpiryChangeRequest>()
            .Include(r => r.Batch)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound(new { message = "Request not found." });
        if (request.Status != "Pending")
            return BadRequest(new { message = "Request already reviewed." });

        request.Status = "Approved";
        request.ReviewedBy = User.Identity?.Name ?? "unknown";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = dto.Notes;

        if (request.Batch != null)
        {
            request.Batch.ExpiryDate = request.NewExpiry;

            // Update main product expiry date to the earliest batch expiry
            var product = await _db.Products
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == request.Batch.ProductId);

            if (product != null)
            {
                var earliestBatch = product.Batches
                    .Where(b => !string.IsNullOrEmpty(b.ExpiryDate))
                    .OrderBy(b => b.ExpiryDate)
                    .FirstOrDefault();

                if (earliestBatch != null)
                {
                    product.ExpiryDate = earliestBatch.ExpiryDate;
                }
            }
        }

        await _db.SaveChangesAsync();

        await _audit.LogAsync("ExpiryChangeApproved", "StockBatch", request.BatchId,
            $"Expiry change approved for batch {request.BatchId}: {request.OldExpiry} → {request.NewExpiry}");

        return Ok(request);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewRequestDto dto)
    {
        var request = await _db.Set<ExpiryChangeRequest>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound(new { message = "Request not found." });
        if (request.Status != "Pending")
            return BadRequest(new { message = "Request already reviewed." });

        request.Status = "Rejected";
        request.ReviewedBy = User.Identity?.Name ?? "unknown";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = dto.Notes;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("ExpiryChangeRejected", "StockBatch", request.BatchId,
            $"Expiry change rejected for batch {request.BatchId}");

        return Ok(request);
    }
}
