using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UserRole { get; set; } = string.Empty;

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
