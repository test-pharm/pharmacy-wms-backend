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
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public ProductsController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _db.Products
            .Include(p => p.Batches)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        foreach (var product in products)
        {
            product.Quantity = product.Batches.Sum(b => b.Quantity);
        }

        return Ok(products);
    }

    [HttpGet("AdminProducts")]
    public async Task<IActionResult> GetAdminProducts()
    {
        var products = await _db.Products
            .Include(p => p.Batches)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        foreach (var product in products)
        {
            product.Quantity = product.Batches.Sum(b => b.Quantity);
        }

        return Ok(new { data = products });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products
            .Include(p => p.Batches)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound(new { message = "Product not found." });

        product.Quantity = product.Batches.Sum(b => b.Quantity);
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            MaterialName = request.MaterialName,
            MaterialSku = request.MaterialSku,
            Quantity = 0,
            Unit = request.Unit,
            LogNumber = request.LogNumber,
            ExpiryDate = request.ExpiryDate,
            Supplier = request.Supplier,
            MinStockLevel = request.MinStockLevel,
            IsAvailable = request.IsAvailable,
            CategoryId = request.CategoryId,
            CategoryName = request.CategoryName ?? "",
            CreatedAt = DateTime.UtcNow,
        };

        if (request.Quantity > 0)
        {
            var expiry = !string.IsNullOrEmpty(request.ExpiryDate) ? request.ExpiryDate : "";
            product.Batches.Add(new StockBatch
            {
                ExpiryDate = expiry,
                Quantity = request.Quantity,
                ReceivedDate = DateTime.UtcNow,
            });
            product.Quantity = request.Quantity;
            product.IsAvailable = true;
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateProduct", "Product", product.Id, $"Created {product.MaterialName} ({product.MaterialSku})");
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _db.Products
            .Include(p => p.Batches)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound(new { message = "Product not found." });

        if (request.MaterialName != null) product.MaterialName = request.MaterialName;
        if (request.MaterialSku != null) product.MaterialSku = request.MaterialSku;
        if (request.Unit != null) product.Unit = request.Unit;
        if (request.LogNumber != null) product.LogNumber = request.LogNumber;
        if (request.ExpiryDate != null) product.ExpiryDate = request.ExpiryDate;
        if (request.Supplier != null) product.Supplier = request.Supplier;
        if (request.MinStockLevel.HasValue) product.MinStockLevel = request.MinStockLevel.Value;
        if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

        if (request.Quantity.HasValue)
        {
            var oldQty = product.Batches.Sum(b => b.Quantity);
            var diff = request.Quantity.Value - oldQty;

            if (diff > 0)
            {
                var expiry = !string.IsNullOrEmpty(request.ExpiryDate) ? request.ExpiryDate : "";
                var existing = product.Batches.FirstOrDefault(b => b.ExpiryDate == expiry);
                if (existing != null)
                    existing.Quantity += diff;
                else
                    product.Batches.Add(new StockBatch
                    {
                        ExpiryDate = expiry,
                        Quantity = diff,
                        ReceivedDate = DateTime.UtcNow,
                    });
            }
            else if (diff < 0)
            {
                var toRemove = -diff;
                foreach (var batch in product.Batches.OrderBy(b => b.ExpiryDate).ThenBy(b => b.Id))
                {
                    if (toRemove <= 0) break;
                    var take = Math.Min(batch.Quantity, toRemove);
                    batch.Quantity -= take;
                    toRemove -= take;
                }
            }

            product.Quantity = request.Quantity.Value;
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("UpdateProduct", "Product", id, $"Updated {product.MaterialName}");
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateProductRequest request)
    {
        return await Patch(id, request);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(p => p.Batches)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound(new { message = "Product not found." });

        var hasOrders = await _db.Orders.AnyAsync(o => o.ProductId == id);
        if (hasOrders)
            return Conflict(new { message = "This product is linked to invoice items." });

        _db.StockBatches.RemoveRange(product.Batches);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DeleteProduct", "Product", id, $"Deleted {product.MaterialName}");
        return NoContent();
    }

    [HttpGet("{id}/batches")]
    public async Task<IActionResult> GetBatches(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        var batches = await _db.StockBatches
            .Where(b => b.ProductId == id && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate == "" ? 1 : 0)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.Id)
            .ToListAsync();

        return Ok(batches);
    }

    [HttpPost("{id}/batches/receive")]
    public async Task<IActionResult> ReceiveStock(int id, [FromBody] ReceiveStockRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be positive." });

        var expiry = (request.ExpiryDate ?? "").Trim();

        var existing = await _db.StockBatches
            .FirstOrDefaultAsync(b => b.ProductId == id && b.ExpiryDate == expiry);

        if (existing != null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            _db.StockBatches.Add(new StockBatch
            {
                ProductId = id,
                ExpiryDate = expiry,
                Quantity = request.Quantity,
                ReceivedDate = DateTime.UtcNow,
            });
        }

        product.Quantity = await _db.StockBatches.Where(b => b.ProductId == id).SumAsync(b => b.Quantity);
        product.IsAvailable = product.Quantity > 0;

        await _db.SaveChangesAsync();
        await _audit.LogAsync("ReceiveStock", "StockBatch", id, $"Received {request.Quantity} of {product.MaterialName} (exp: {expiry})");
        return Ok(new { message = "Stock received.", totalQuantity = product.Quantity });
    }

    [HttpGet("{id}/batches/fefo")]
    public async Task<IActionResult> GetFefoBatches(int id, [FromQuery] int quantity)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        var available = await _db.StockBatches
            .Where(b => b.ProductId == id && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate == "" ? 1 : 0)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.Id)
            .ToListAsync();

        var total = available.Sum(b => b.Quantity);
        if (quantity > total)
            return BadRequest(new { message = $"Insufficient stock. Available: {total}, Requested: {quantity}" });

        var result = new List<object>();
        var remaining = quantity;

        foreach (var batch in available)
        {
            if (remaining <= 0) break;
            var take = Math.Min(batch.Quantity, remaining);
            result.Add(new { batchId = batch.Id, expiryDate = batch.ExpiryDate, quantity = take });
            remaining -= take;
        }

        return Ok(new { productId = id, productName = product.MaterialName, totalQuantity = quantity, batches = result });
    }
}
