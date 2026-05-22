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
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _db.Products
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("AdminProducts")]
    public async Task<IActionResult> GetAdminProducts()
    {
        var products = await _db.Products
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(new { data = products });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            MaterialName = request.MaterialName,
            MaterialSku = request.MaterialSku,
            Quantity = request.Quantity,
            Unit = request.Unit,
            LogNumber = request.LogNumber,
            ExpiryDate = request.ExpiryDate,
            StorageLocation = request.StorageLocation,
            IsAvailable = request.IsAvailable,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        if (request.MaterialName != null) product.MaterialName = request.MaterialName;
        if (request.MaterialSku != null) product.MaterialSku = request.MaterialSku;
        if (request.Quantity.HasValue) product.Quantity = request.Quantity.Value;
        if (request.Unit != null) product.Unit = request.Unit;
        if (request.LogNumber != null) product.LogNumber = request.LogNumber;
        if (request.ExpiryDate != null) product.ExpiryDate = request.ExpiryDate;
        if (request.StorageLocation != null) product.StorageLocation = request.StorageLocation;
        if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

        await _db.SaveChangesAsync();
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
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "Product not found." });

        var hasOrders = await _db.Orders.AnyAsync(o => o.ProductId == id);
        if (hasOrders)
            return Conflict(new { message = "This product is linked to invoice items." });

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
