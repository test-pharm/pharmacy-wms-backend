using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.Models;

public class Contact
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Type { get; set; } = "Supplier"; // "Supplier" or "Recipient"

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
