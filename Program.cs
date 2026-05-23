using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PharmacyWmsBackend.Data;
using PharmacyWmsBackend.Models;
using PharmacyWmsBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// â”€â”€ Database â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Uses SQLite (file-based, no server needed). Works on somee.com out of the
// box. The SQL Server provider is kept for future migration if needed.
var connStr = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connStr);
});

// â”€â”€ JWT Auth â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

// â”€â”€ Services â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<ResetCodeService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<AuditLogCleanupService>();

// â”€â”€ CORS (allow Flutter desktop) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// â”€â”€ Auto-create DB + seed â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Migration: add InvoiceNumber and ExpiryDate columns if missing
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN InvoiceNumber TEXT NULL"); } catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN ExpiryDate TEXT NULL"); } catch { }

    // Migration: copy existing stock into StockBatch for FEFO
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Products ADD COLUMN BatchesTemp INT NULL"); } catch { }
    try { db.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS StockBatchesTemp"); } catch { }
    try
    {
        var legacyProducts = db.Products.Where(p => p.Quantity > 0).ToList();
        foreach (var p in legacyProducts)
        {
            var hasBatch = db.StockBatches.Any(b => b.ProductId == p.Id);
            if (!hasBatch)
            {
                db.StockBatches.Add(new StockBatch
                {
                    ProductId = p.Id,
                    ExpiryDate = p.ExpiryDate ?? "",
                    Quantity = p.Quantity,
                    ReceivedDate = DateTime.UtcNow,
                });
            }
        }
        await db.SaveChangesAsync();
    }
    catch { }

    await DbSeeder.SeedAsync(db);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/version", () => Results.Ok(new
{
    version = "1.0.7",
    build = 2,
    environment = app.Environment.EnvironmentName
}));

app.Run();
