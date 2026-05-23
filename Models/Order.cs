using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyWmsBackend.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    public int? ProductId { get; set; }

    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ProductSku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LogNumber { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [Required, MaxLength(20)]
    public string Type { get; set; } = "add"; // add, export, edit

    [Required, MaxLength(20)]
    public string Status { get; set; } = "pending"; // completed, pending, canceled

    [MaxLength(200)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    [MaxLength(50)]
    public string? ExpiryDate { get; set; }
}
