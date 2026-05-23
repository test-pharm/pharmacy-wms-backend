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
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly ResetCodeService _resetCode;
    private readonly EmailService _email;
    private readonly AuditLogService _audit;

    public AuthController(AppDbContext db, TokenService tokenService,
        ResetCodeService resetCode, EmailService email, AuditLogService audit)
    {
        _db = db;
        _tokenService = tokenService;
        _resetCode = resetCode;
        _email = email;
        _audit = audit;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !PasswordService.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _tokenService.GenerateToken(user);
        await _audit.LogAsync("Login", "User", user.Id, $"User {user.Email} logged in");
        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Token = token,
            }
        });
    }

    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            Email = request.Email,
            PasswordHash = PasswordService.Hash(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = "Admin",
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("RegisterAdmin", "User", user.Id, $"Registered admin {user.Email}");
        return Ok(new { message = "Admin registered successfully." });
    }

    [HttpPost("register/user")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            Email = request.Email,
            PasswordHash = PasswordService.Hash(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = "User",
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("RegisterUser", "User", user.Id, $"Registered user {user.Email}");
        return Ok(new { message = "User registered successfully." });
    }

    [Authorize]
    [HttpPatch("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;

        await _db.SaveChangesAsync();
        await _audit.LogAsync("UpdateProfile", "User", userId, "Profile updated");
        return Ok(new { message = "Profile updated." });
    }

    [HttpGet("supervisors")]
    public async Task<IActionResult> GetSupervisors()
    {
        var supervisors = await _db.Users
            .Where(u => u.Role == "User")
            .Select(u => new { u.Email, u.FullName })
            .ToListAsync();

        return Ok(supervisors);
    }

    [HttpPost("send-reset-code")]
    public async Task<IActionResult> SendResetCode([FromBody] SendResetCodeRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return Ok(new { message = "If the email exists, a code has been sent." });

        var code = _resetCode.GenerateCode(request.Email);
        await _email.SendEmailAsync(request.Email, "Password Reset Code",
            $"Your password reset code is: {code}. It expires in 15 minutes.");

        return Ok(new { message = "If the email exists, a code has been sent." });
    }

    [HttpPost("verify-reset-code")]
    public IActionResult VerifyResetCode([FromBody] VerifyResetCodeRequest request)
    {
        var valid = _resetCode.VerifyCode(request.Email, request.Code);
        if (!valid)
            return BadRequest(new { message = "Invalid or expired code." });

        return Ok(new { message = "Code verified." });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var valid = _resetCode.VerifyCode(request.Email, request.Code);
        if (!valid)
            return BadRequest(new { message = "Invalid or expired code." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound(new { message = "User not found." });

        user.PasswordHash = PasswordService.Hash(request.NewPassword);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("ChangePassword", "User", user.Id, $"Password changed for {request.Email}");
        return Ok(new { message = "Password changed successfully." });
    }
}
