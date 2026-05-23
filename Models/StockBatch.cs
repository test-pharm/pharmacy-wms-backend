using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyWmsBackend.Models;

public class StockBatch
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    [MaxLength(50)]
    public string ExpiryDate { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}
