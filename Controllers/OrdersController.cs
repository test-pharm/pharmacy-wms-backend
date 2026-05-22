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
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

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
            CreatedAt = DateTime.UtcNow,
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound(new { message = "Order not found." });

        order.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(order);
    }
}
