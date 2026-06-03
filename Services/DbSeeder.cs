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

        // Seed supervisor if not exists
        if (!await db.Users.AnyAsync(u => u.Email == "supervisor@pharmacy.com"))
        {
            db.Users.Add(new User
            {
                Email = "supervisor@pharmacy.com",
                PasswordHash = PasswordService.Hash("super123"),
                FullName = "Warehouse Supervisor",
                PhoneNumber = "01000000001",
                Role = "Supervisor",
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        // Seed sample products if empty
        if (!await db.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new() { MaterialName = "Paracetamol 500mg", MaterialSku = "PCM-500", Quantity = 500, Unit = "Tablet", LogNumber = "LOG-001", ExpiryDate = "2027-06-15", Supplier = "PharmaGlobal Inc.", IsAvailable = true, CategoryId = 1, CategoryName = "Analgesics", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Amoxicillin 250mg", MaterialSku = "AMX-250", Quantity = 300, Unit = "Capsule", LogNumber = "LOG-002", ExpiryDate = "2026-12-20", Supplier = "MediSupply Co.", IsAvailable = true, CategoryId = 2, CategoryName = "Antibiotics", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Omeprazole 20mg", MaterialSku = "OMP-20", Quantity = 200, Unit = "Capsule", LogNumber = "LOG-003", ExpiryDate = "2027-03-10", Supplier = "HealthCorp Ltd.", IsAvailable = true, CategoryId = 3, CategoryName = "GI Medications", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Salbutamol Inhaler", MaterialSku = "SAL-INH", Quantity = 50, Unit = "Inhaler", LogNumber = "LOG-004", ExpiryDate = "2026-08-01", Supplier = "RespCare GmbH", IsAvailable = true, CategoryId = 4, CategoryName = "Respiratory", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Metformin 500mg", MaterialSku = "MET-500", Quantity = 400, Unit = "Tablet", LogNumber = "LOG-005", ExpiryDate = "2027-09-25", Supplier = "DiabeWell Pharma", IsAvailable = true, CategoryId = 5, CategoryName = "Diabetes", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Atorvastatin 10mg", MaterialSku = "ATV-10", Quantity = 350, Unit = "Tablet", LogNumber = "LOG-006", ExpiryDate = "2027-01-15", Supplier = "CardioHealth Labs", IsAvailable = true, CategoryId = 6, CategoryName = "Cardiovascular", CreatedAt = DateTime.UtcNow },
                new() { MaterialName = "Ibuprofen 400mg", MaterialSku = "IBU-400", Quantity = 600, Unit = "Tablet", LogNumber = "LOG-007", ExpiryDate = "2026-11-30", Supplier = "PharmaGlobal Inc.", IsAvailable = true, CategoryId = 1, CategoryName = "Analgesics", CreatedAt = DateTime.UtcNow },
            };
            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }
    }
}
