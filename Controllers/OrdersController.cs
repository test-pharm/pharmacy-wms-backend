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
            CreatedAt = DateTime.UtcNow,
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateOrder", "Order", order.Id, $"Created {order.Type} order for {order.ProductName} x{order.Quantity}");
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
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
}
