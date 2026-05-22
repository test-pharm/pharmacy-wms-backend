using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.DTOs;

public class CreateNotificationRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MaterialName { get; set; }

    [MaxLength(100)]
    public string? ProductSku { get; set; }

    [MaxLength(50)]
    public string? ProposedExpiry { get; set; }

    [MaxLength(200)]
    public string? ManagerName { get; set; }
}
