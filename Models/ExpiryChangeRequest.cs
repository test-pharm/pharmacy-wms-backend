using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyWmsBackend.Models;

public class ExpiryChangeRequest
{
    [Key]
    public int Id { get; set; }

    public int BatchId { get; set; }

    [MaxLength(50)]
    public string OldExpiry { get; set; } = string.Empty;

    [MaxLength(50)]
    public string NewExpiry { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(100)]
    public string RequestedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? ReviewNotes { get; set; }

    [ForeignKey(nameof(BatchId))]
    public StockBatch? Batch { get; set; }
}
