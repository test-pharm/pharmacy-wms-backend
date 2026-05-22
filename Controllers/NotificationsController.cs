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
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public NotificationsController(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _db.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            Title = request.Title,
            Body = request.Body,
            MaterialName = request.MaterialName,
            ProductSku = request.ProductSku,
            ProposedExpiry = request.ProposedExpiry,
            ManagerName = request.ManagerName,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = notification.Id }, notification);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await _db.SaveChangesAsync();

        return Ok(notification);
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _db.Notifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

        return Ok(new { message = "All notifications marked as read." });
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        await _email.SendEmailAsync(request.To, request.Subject, request.Body);
        return Ok(new { message = "Email sent." });
    }
}
