using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyWmsBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private static DateTime _lastRead = DateTime.MinValue;
    private static UpdateInfo? _cached;

    private static string? FindVersionFile()
    {
        var candidates = new[]
        {
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json"),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "version.json"),
        };
        foreach (var p in candidates)
        {
            if (System.IO.File.Exists(p)) return p;
        }
        return null;
    }

    private static UpdateInfo GetInfo()
    {
        var elapsed = (DateTime.UtcNow - _lastRead).TotalSeconds;
        if (_cached != null && elapsed < 30) return _cached;

        var path = FindVersionFile();
        if (path != null)
        {
            try
            {
                var json = System.IO.File.ReadAllText(path);
                _cached = JsonSerializer.Deserialize<UpdateInfo>(json) ?? new UpdateInfo();
                _lastRead = DateTime.UtcNow;
                return _cached;
            }
            catch { }
        }
        return new UpdateInfo();
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
            releaseNotes = info.ReleaseNotes,
        });
    }

    [HttpGet("debug")]
    public IActionResult Debug()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var curDir = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            System.IO.Path.Combine(baseDir, "version.json"),
            System.IO.Path.Combine(curDir, "version.json"),
        };
        return Ok(new
        {
            baseDirectory = baseDir,
            currentDirectory = curDir,
            files = candidates.Select(p => new { path = p, exists = System.IO.File.Exists(p) }),
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
