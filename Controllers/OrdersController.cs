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
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public OrdersController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound(new { message = "Order not found." });
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            ProductSku = request.ProductSku,
            Quantity = request.Quantity,
            Unit = request.Unit,
            LogNumber = request.LogNumber,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Status = "completed",
            CreatedBy = request.CreatedBy,
            Notes = request.Notes,
            InvoiceNumber = request.InvoiceNumber,
            ExpiryDate = request.ExpiryDate,
            CreatedAt = DateTime.UtcNow,
        };

        if (request.Type == "export" && request.ProductId.HasValue)
        {
            var batches = await _db.StockBatches
                .Where(b => b.ProductId == request.ProductId && b.Quantity > 0)
                .OrderBy(b => b.ExpiryDate == "" ? 1 : 0)
                .ThenBy(b => b.ExpiryDate)
                .ThenBy(b => b.Id)
                .ToListAsync();

            var total = batches.Sum(b => b.Quantity);
            if (request.Quantity > total)
                return BadRequest(new { message = $"Insufficient stock. Available: {total}, Requested: {request.Quantity}" });

            var remaining = request.Quantity;
            foreach (var batch in batches)
            {
                if (remaining <= 0) break;
                var take = Math.Min(batch.Quantity, remaining);
                batch.Quantity -= take;
                remaining -= take;
            }

            if (order.ExpiryDate == null)
            {
                var firstBatch = batches.FirstOrDefault(b => !string.IsNullOrEmpty(b.ExpiryDate));
                if (firstBatch != null)
                    order.ExpiryDate = firstBatch.ExpiryDate;
            }

            var product = await _db.Products.FindAsync(request.ProductId);
            if (product != null)
            {
                product.Quantity = batches.Sum(b => b.Quantity);
                product.IsAvailable = product.Quantity > 0;
            }
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateOrder", "Order", order.Id, $"Created {order.Type} order for {order.ProductName} x{order.Quantity}");
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] DispatchStockRequest request)
    {
        if (request.ProductId == null)
            return BadRequest(new { message = "ProductId is required." });

        var product = await _db.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound(new { message = "Product not found." });

        var batches = await _db.StockBatches
            .Where(b => b.ProductId == request.ProductId && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate == "" ? 1 : 0)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.Id)
            .ToListAsync();

        var total = batches.Sum(b => b.Quantity);
        if (request.Quantity > total)
            return BadRequest(new { message = $"Insufficient stock. Available: {total}, Requested: {request.Quantity}" });

        var remaining = request.Quantity;
        var batchBreakdown = new List<object>();

        foreach (var batch in batches)
        {
            if (remaining <= 0) break;
            var take = Math.Min(batch.Quantity, remaining);
            batch.Quantity -= take;
            remaining -= take;
            batchBreakdown.Add(new { batchId = batch.Id, expiryDate = batch.ExpiryDate, quantity = take });
        }

        product.Quantity = batches.Sum(b => b.Quantity);
        product.IsAvailable = product.Quantity > 0;

        var order = new Order
        {
            ProductId = request.ProductId,
            ProductName = product.MaterialName,
            ProductSku = product.MaterialSku,
            Quantity = request.Quantity,
            Unit = product.Unit,
            LogNumber = product.LogNumber,
            CategoryId = product.CategoryId,
            Type = "export",
            Status = "completed",
            CreatedBy = request.CreatedBy ?? "system",
            Notes = request.Notes,
            InvoiceNumber = request.InvoiceNumber,
            ExpiryDate = batches.FirstOrDefault(b => !string.IsNullOrEmpty(b.ExpiryDate))?.ExpiryDate,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("DispatchFefo", "Order", order.Id, $"Dispatched {request.Quantity} of {product.MaterialName} from {batchBreakdown.Count} batch(es)");

        return Ok(new
        {
            orderId = order.Id,
            productName = product.MaterialName,
            totalQuantity = request.Quantity,
            batches = batchBreakdown,
        });
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await _db.Orders
            .Where(o => o.InvoiceNumber != null && o.InvoiceNumber != "")
            .GroupBy(o => o.InvoiceNumber)
            .Select(g => new
            {
                invoiceNumber = g.Key,
                materialCount = g.Count(),
                totalQuantity = g.Sum(o => o.Quantity),
                dateFrom = g.Min(o => o.CreatedAt),
                dateTo = g.Max(o => o.CreatedAt),
            })
            .OrderByDescending(i => i.dateTo)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("invoices/{invoiceNumber}")]
    public async Task<IActionResult> GetInvoiceDetails(string invoiceNumber)
    {
        var orders = await _db.Orders
            .Where(o => o.InvoiceNumber == invoiceNumber)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        if (!orders.Any()) return NotFound(new { message = "Invoice not found." });
        return Ok(orders);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound(new { message = "Order not found." });

        order.Status = request.Status;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("UpdateOrderStatus", "Order", id, $"Order {id} status changed to {request.Status}");
        return Ok(order);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound(new { message = "Order not found." });

        if (order.Status == "canceled")
            return BadRequest(new { message = "Order is already canceled." });

        if (order.Type == "export" && order.ProductId.HasValue)
        {
            var existing = await _db.StockBatches
                .FirstOrDefaultAsync(b => b.ProductId == order.ProductId && b.ExpiryDate == (order.ExpiryDate ?? ""));
            if (existing != null)
                existing.Quantity += order.Quantity;
            else
            {
                _db.StockBatches.Add(new StockBatch
                {
                    ProductId = order.ProductId.Value,
                    ExpiryDate = order.ExpiryDate ?? "",
                    Quantity = order.Quantity,
                    ReceivedDate = DateTime.UtcNow,
                });
            }

            var product = await _db.Products.FindAsync(order.ProductId);
            if (product != null)
            {
                product.Quantity = await _db.StockBatches.Where(b => b.ProductId == product.Id).SumAsync(b => b.Quantity);
                product.IsAvailable = product.Quantity > 0;
            }
        }

        order.Status = "canceled";
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CancelOrder", "Order", id, $"Order {id} canceled. Stock returned.");
        return Ok(order);
    }
}
