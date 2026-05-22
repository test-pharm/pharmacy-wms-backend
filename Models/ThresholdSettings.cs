using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.Models;

public class ThresholdSettings
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public int Value { get; set; }
}
