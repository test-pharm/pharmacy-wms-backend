using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.DTOs;

public class CreateProductRequest
{
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
}

public class UpdateProductRequest
{
    [MaxLength(200)]
    public string? MaterialName { get; set; }

    [MaxLength(100)]
    public string? MaterialSku { get; set; }

    public int? Quantity { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(100)]
    public string? LogNumber { get; set; }

    [MaxLength(50)]
    public string? ExpiryDate { get; set; }

    [MaxLength(200)]
    public string? StorageLocation { get; set; }

    public bool? IsAvailable { get; set; }

    public int? CategoryId { get; set; }
}
