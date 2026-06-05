using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = "User"; // Admin or User

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
