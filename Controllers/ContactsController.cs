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
public class ContactsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public ContactsController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? type)
    {
        var query = _db.Contacts.AsQueryable();
        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(c => c.Type.ToLower() == type.ToLower());
        }

        var contacts = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact == null) return NotFound();
        return Ok(contact);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Contact name is required." });

        var validTypes = new[] { "Supplier", "Recipient" };
        if (string.IsNullOrEmpty(request.Type) || !validTypes.Contains(request.Type))
            return BadRequest(new { message = "Type must be 'Supplier' or 'Recipient'." });

        var contact = new Contact
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Phone = request.Phone?.Trim(),
            Notes = request.Notes?.Trim(),
        };

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("CreateContact", "Contact", contact.Id, $"Created contact: {contact.Name} ({contact.Type})");
        return Ok(contact);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ContactRequest request)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact == null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Contact name is required." });

        var validTypes = new[] { "Supplier", "Recipient" };
        if (string.IsNullOrEmpty(request.Type) || !validTypes.Contains(request.Type))
            return BadRequest(new { message = "Type must be 'Supplier' or 'Recipient'." });

        var oldName = contact.Name;
        var oldType = contact.Type;

        contact.Name = request.Name.Trim();
        contact.Type = request.Type;
        contact.Phone = request.Phone?.Trim();
        contact.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync();

        await _audit.LogAsync("UpdateContact", "Contact", contact.Id, $"Updated contact {oldName} ({oldType}) -> {contact.Name} ({contact.Type})");
        return Ok(contact);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact == null) return NotFound();

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("DeleteContact", "Contact", id, $"Deleted contact: {contact.Name}");
        return Ok(new { message = "Contact deleted successfully." });
    }
}

public class ContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Supplier"; // "Supplier" or "Recipient"
    public string? Phone { get; set; }
    public string? Notes { get; set; }
}
