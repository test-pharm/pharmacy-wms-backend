using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private static DateTime _lastRead = DateTime.MinValue;
    private static UpdateInfo? _cached;

    private static UpdateInfo GetInfo()
    {
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json");
        if (System.IO.File.Exists(path) && (DateTime.UtcNow - _lastRead).TotalSeconds > 30)
        {
            var json = System.IO.File.ReadAllText(path);
            _cached = JsonSerializer.Deserialize<UpdateInfo>(json) ?? new UpdateInfo();
            _lastRead = DateTime.UtcNow;
        }
        return _cached ?? new UpdateInfo();
    }

    [HttpGet]
    public IActionResult Get()
    {
        var info = GetInfo();
        return Ok(new
        {
            latestVersion = info.LatestVersion,
            latestBuildNumber = info.LatestBuildNumber,
            downloadUrl = info.DownloadUrl,
            mandatory = info.Mandatory,
            releaseNotes = info.ReleaseNotes
        });
    }

    private class UpdateInfo
    {
        public string LatestVersion { get; set; } = "1.0.0";
        public int LatestBuildNumber { get; set; } = 0;
        public string DownloadUrl { get; set; } = "https://github.com/test-pharm/pharmacy-wms-flutter/releases/latest/download/pharmacy-wms-windows.zip";
        public bool Mandatory { get; set; } = false;
        public List<string> ReleaseNotes { get; set; } = new();
    }
}
