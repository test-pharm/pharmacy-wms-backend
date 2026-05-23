using Microsoft.AspNetCore.Mvc;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            latestVersion = "1.0.9",
            latestBuildNumber = 1,
            downloadUrl = "https://github.com/test-pharm/pharmacy-wms-flutter/releases/latest/download/pharmacy-wms-windows.zip",
            mandatory = false,
            releaseNotes = new[]
            {
                "Invoices tab with grouped order view and detail dialog",
                "Audit log tab restored in sidebar navigation",
                "Proper InvoiceNumber/ExpiryDate database columns",
                "Backend-driven update checks (no GitHub URL dependency)",
                "Fixed order creation: productId type mismatch resolved",
                "Database migration for InvoiceNumber/ExpiryDate columns",
            }
        });
    }
}
