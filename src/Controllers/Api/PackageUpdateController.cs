using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Services;

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
