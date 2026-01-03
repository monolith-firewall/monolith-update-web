using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Services;

namespace MonolithUpdateSite.Controllers.Api;

[ApiController]
[Route("api/v1/firewall")]
public class FirewallUpdateController : ControllerBase
{
    private readonly IUpdateService _updateService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _context;

    public FirewallUpdateController(
        IUpdateService updateService,
        IFileStorageService fileStorageService,
        ApplicationDbContext context)
    {
        _updateService = updateService;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    [HttpGet("check-update")]
    public IActionResult CheckUpdate([FromQuery] string currentVersion, [FromQuery] string? hardwareId = null)
    {
        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            return BadRequest(new { error = "Current version is required." });
        }

        var response = _updateService.CheckFirewallUpdate(currentVersion);
        return Ok(response);
    }

    [HttpGet("download/{version}")]
    public async Task<IActionResult> Download(string version)
    {
        var firewallVersion = await _context.FirewallVersions
            .FirstOrDefaultAsync(v => v.Version == version);

        if (firewallVersion == null)
        {
            return NotFound(new { error = "Version not found." });
        }

        var update = await _context.FirewallUpdates
            .FirstOrDefaultAsync(u => u.FirewallVersionId == firewallVersion.Id && u.IsActive);

        if (update == null)
        {
            return NotFound(new { error = "Update not found." });
        }

        var filePath = _fileStorageService.GetFirewallUpdatePath(update.FileName);

        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "Update file not found." });
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "application/octet-stream", update.FileName);
    }
}
