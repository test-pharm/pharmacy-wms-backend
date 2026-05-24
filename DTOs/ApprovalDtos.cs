using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.DTOs;

public class ExpiryChangeRequestDto
{
    public int BatchId { get; set; }
    public string NewExpiry { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ReviewRequestDto
{
    public bool Approved { get; set; }
    public string? Notes { get; set; }
}
