using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private static UpdateInfo? _cached;

    private static UpdateInfo GetInfo()
    {
        if (_cached != null) return _cached;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json");
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _cached = JsonSerializer.Deserialize<UpdateInfo>(json) ?? new UpdateInfo();
        }
        else
        {
            _cached = new UpdateInfo();
        }
        return _cached;
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
