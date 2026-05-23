using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyWmsBackend.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string MaterialName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string MaterialSku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LogNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ExpiryDate { get; set; } = string.Empty;

    [MaxLength(200)]
    public string StorageLocation { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public int CategoryId { get; set; }

    [MaxLength(200)]
    public string? CategoryName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<StockBatch> Batches { get; set; } = new();
}
