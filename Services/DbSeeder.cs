using Microsoft.EntityFrameworkCore;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.Models;

namespace PharmacyWmsBackend.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Seed admin if not exists
        if (!await db.Users.AnyAsync(u => u.Email == "admin@pharmacy.com"))
        {
            db.Users.Add(new User
            {
                Email = "admin@pharmacy.com",
                PasswordHash = PasswordService.Hash("admin123"),
                FullName = "Admin Manager",
                PhoneNumber = "01000000000",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }
    }
}
