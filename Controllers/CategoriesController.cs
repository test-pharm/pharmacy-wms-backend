using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.Models;
using PharmacyWmsBackend.Services;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public CategoriesController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Category name is required." });

        var nameNormalized = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == nameNormalized.ToLower());
        if (exists)
            return BadRequest(new { message = "A category with this name already exists." });

        var category = new Category
        {
            Name = nameNormalized,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateCategory", "Category", category.Id, $"Created category: {category.Name}");
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Category name is required." });

        var nameNormalized = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == nameNormalized.ToLower());
        if (exists)
            return BadRequest(new { message = "A category with this name already exists." });

        var oldName = category.Name;
        category.Name = nameNormalized;

        await _db.SaveChangesAsync();

        // Update CategoryName across products referencing this category id
        var products = await _db.Products.Where(p => p.CategoryId == id).ToListAsync();
        foreach (var p in products)
        {
            p.CategoryName = nameNormalized;
        }
        if (products.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        await _audit.LogAsync("UpdateCategory", "Category", category.Id, $"Updated category {oldName} -> {category.Name}");
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        // Check if category is used by any products
        var isUsed = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (isUsed)
            return BadRequest(new { message = "Cannot delete category because it is associated with existing materials." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("DeleteCategory", "Category", id, $"Deleted category: {category.Name}");
        return Ok(new { message = "Category deleted successfully." });
    }
}

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
}
