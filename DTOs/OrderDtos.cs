using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.DTOs;

public class CreateOrderRequest
{
    public int? ProductId { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ProductSku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LogNumber { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [Required, MaxLength(20)]
    public string Type { get; set; } = "add";

    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(200)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    [MaxLength(50)]
    public string? ExpiryDate { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
