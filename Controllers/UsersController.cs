using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.Models;
using PharmacyWmsBackend.Services;
using System.Security.Claims;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _audit;

    public UsersController(AppDbContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.IsActive,
                u.PhoneNumber,
                u.CreatedAt,
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPatch("{id}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var validRoles = new[] { "Admin", "User" };
        if (string.IsNullOrEmpty(request.Role) || !validRoles.Contains(request.Role))
            return BadRequest(new { message = "Invalid role. Must be 'Admin' or 'User'." });

        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized();

        if (user.Id == currentUserId)
            return BadRequest(new { message = "Cannot change your own role." });

        var oldRole = user.Role;
        user.Role = request.Role;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("ChangeUserRole", "User", user.Id, $"Changed role of {user.FullName} ({user.Email}) from {oldRole} to {request.Role}");
        return Ok(new { message = "Role updated successfully." });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized();

        if (user.Id == currentUserId)
            return BadRequest(new { message = "Cannot deactivate yourself." });

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        var action = user.IsActive ? "ActivateUser" : "DeactivateUser";
        await _audit.LogAsync(action, "User", user.Id, $"{(user.IsActive ? "Activated" : "Deactivated")} user: {user.FullName} ({user.Email})");

        return Ok(new { message = user.IsActive ? "User activated successfully." : "User deactivated successfully.", isActive = user.IsActive });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out var currentUserId))
            return Unauthorized();

        if (user.Id == currentUserId)
            return BadRequest(new { message = "Cannot delete yourself." });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("DeleteUser", "User", id, $"Deleted user: {user.FullName} ({user.Email})");
        return Ok(new { message = "User deleted successfully." });
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        user.PasswordHash = PasswordService.Hash(request.NewPassword);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("ResetUserPassword", "User", user.Id, $"Reset password for user: {user.FullName} ({user.Email})");
        return Ok(new { message = "Password reset successfully." });
    }
}

public class ChangeRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
