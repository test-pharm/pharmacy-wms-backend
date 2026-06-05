using System.ComponentModel.DataAnnotations;

namespace PharmacyWmsBackend.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
