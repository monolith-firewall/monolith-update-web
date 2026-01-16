using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Services;
using MonolithUpdateSite.Models.Domain;

namespace MonolithUpdateSite.Controllers.Api;

[ApiController]
[Route("api/v1/packages")]
public class PackageUpdateController : ControllerBase
{
    private readonly IUpdateService _updateService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _context;

    public PackageUpdateController(
        IUpdateService updateService,
        IFileStorageService fileStorageService,
        ApplicationDbContext context)
    {
        _updateService = updateService;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    /// <summary>
    /// Package feed endpoint consumed by Monolith.FireWall.WebUI (PackageUpdatesClient).
    ///
    /// WebUI calls: GET {BaseUrl}?version={coreVersion}
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Feed([FromQuery] string? version = null)
    {
        try
        {
            // In the firewall WebUI, the query param is named "version".
            // This corresponds to the firewall/core version. We reuse it as our compatibility filter.
            var firewallVersion = version;

            // Try to query normally first, but handle missing Category column
            List<MonolithPackage> packages;
            try
            {
                packages = await _context.MonolithPackages
                    .Include(p => p.PackageUpdates)
                        .ThenInclude(u => u.RequiredFirewallVersion)
                    .Where(p => p.IsActive)
                    .ToListAsync();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such column") && ex.Message.Contains("Category"))
            {
                // Category column doesn't exist - add it and retry
                try
                {
                    // SQLite: Add column with default value
                    // If column already exists, this will fail but that's OK
                    await _context.Database.ExecuteSqlRawAsync(@"
                        ALTER TABLE MonolithPackages 
                        ADD COLUMN Category TEXT DEFAULT 'Other';
                    ");
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    // Column might already exist (race condition), that's OK
                }
                
                // Retry the query (column should exist now, or it was added by another request)
                packages = await _context.MonolithPackages
                    .Include(p => p.PackageUpdates)
                        .ThenInclude(u => u.RequiredFirewallVersion)
                    .Where(p => p.IsActive)
                    .ToListAsync();
            }

            var hostBase = $"{Request.Scheme}://{Request.Host}";

            var results = new List<object>();
            foreach (var package in packages)
            {
                try
                {
                    var latestUpdate = package.PackageUpdates
                        .Where(u => u.IsActive)
                        .OrderByDescending(u => u.ReleaseDate)
                        .FirstOrDefault();

                    if (latestUpdate == null)
                    {
                        continue;
                    }

                    // Skip if required fields are missing
                    if (string.IsNullOrWhiteSpace(package.PackageCode) || 
                        string.IsNullOrWhiteSpace(latestUpdate.Version))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(firewallVersion) && 
                        latestUpdate.RequiredFirewallVersion != null &&
                        !string.IsNullOrWhiteSpace(latestUpdate.RequiredFirewallVersion.Version))
                    {
                        var required = latestUpdate.RequiredFirewallVersion.Version;
                        var compatible =
                            _updateService.CompareVersions(firewallVersion, required) ||
                            string.Equals(firewallVersion, required, StringComparison.Ordinal);

                        if (!compatible)
                        {
                            continue;
                        }
                    }

                    var downloadUrl = $"{hostBase}/api/v1/packages/download/{package.PackageCode}/{latestUpdate.Version}";

                    results.Add(new
                    {
                        id = package.PackageCode,
                        name = package.PackageName ?? package.PackageCode,
                        version = latestUpdate.Version,
                        description = package.Description,
                        category = package.Category ?? "Other",
                        downloadUrl,
                        sha256 = string.IsNullOrWhiteSpace(latestUpdate.FileHash) ? null : latestUpdate.FileHash,
                        releaseNotes = string.IsNullOrWhiteSpace(latestUpdate.ReleaseNotes) ? null : latestUpdate.ReleaseNotes,
                        minCoreVersion = latestUpdate.RequiredFirewallVersion?.Version,
                        requiresRestart = false
                    });
                }
                catch (Exception ex)
                {
                    // Log and skip problematic packages
                    // In production, you might want to log this
                    continue;
                }
            }

            return Ok(new { packages = results });
        }
        catch (Exception ex)
        {
            // Return 500 with error details (in production, you might want to hide details)
            return StatusCode(500, new { error = "An error occurred while fetching packages.", message = ex.Message });
        }
    }

    [HttpGet("check-update")]
    public IActionResult CheckUpdate(
        [FromQuery] string packageCode,
        [FromQuery] string currentVersion,
        [FromQuery] string? firewallVersion = null)
    {
        if (string.IsNullOrWhiteSpace(packageCode))
        {
            return BadRequest(new { error = "Package code is required." });
        }

        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            return BadRequest(new { error = "Current version is required." });
        }

        var response = _updateService.CheckPackageUpdate(packageCode, currentVersion, firewallVersion);
        return Ok(response);
    }

    [HttpGet("download/{packageCode}/{version}")]
    public async Task<IActionResult> Download(string packageCode, string version)
    {
        var package = await _context.MonolithPackages
            .Include(p => p.PackageUpdates)
            .FirstOrDefaultAsync(p => p.PackageCode == packageCode);

        if (package == null)
        {
            return NotFound(new { error = "Package not found." });
        }

        var update = package.PackageUpdates
            .FirstOrDefault(u => u.Version == version && u.IsActive);

        if (update == null)
        {
            return NotFound(new { error = "Package update not found." });
        }

        var filePath = _fileStorageService.GetPackageUpdatePath(packageCode, update.FileName);

        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "Package file not found." });
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "application/octet-stream", update.FileName);
    }

    [HttpGet("list")]
    public IActionResult List([FromQuery] string? firewallVersion = null)
    {
        var response = _updateService.GetAvailablePackages(firewallVersion);
        return Ok(response);
    }
}
