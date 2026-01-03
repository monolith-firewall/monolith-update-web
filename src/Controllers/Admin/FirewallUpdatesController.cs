using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;
using MonolithUpdateSite.Services;

namespace MonolithUpdateSite.Controllers.Admin;

[Authorize]
[Route("Admin/[controller]")]
public class FirewallUpdatesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public FirewallUpdatesController(ApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var updates = await _context.FirewallUpdates
            .Include(u => u.FirewallVersion)
            .Include(u => u.MinimumFirewallVersion)
            .OrderByDescending(u => u.ReleaseDate)
            .ToListAsync();
        return View(updates);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new FirewallUpdateViewModel
        {
            AvailableFirewallVersions = await GetFirewallVersionSelectListAsync(),
            MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true)
        };
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FirewallUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            model.MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true);
            return View(model);
        }

        if (model.UpdateFile == null)
        {
            ModelState.AddModelError("UpdateFile", "Update file is required.");
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            model.MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true);
            return View(model);
        }

        var version = await _context.FirewallVersions.FindAsync(model.FirewallVersionId);
        if (version == null)
        {
            ModelState.AddModelError("FirewallVersionId", "Invalid firewall version selected.");
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            model.MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true);
            return View(model);
        }

        var (fileName, fileSize, fileHash) = await _fileStorageService.SaveFirewallUpdateAsync(model.UpdateFile, version.Version);

        var update = new FirewallUpdate
        {
            FirewallVersionId = model.FirewallVersionId,
            ReleaseDate = model.ReleaseDate,
            FileName = fileName,
            FileSize = fileSize,
            FileHash = fileHash,
            ReleaseNotes = model.ReleaseNotes,
            IsActive = model.IsActive,
            IsCritical = model.IsCritical,
            MinimumFirewallVersionId = model.MinimumFirewallVersionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FirewallUpdates.Add(update);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall update created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var update = await _context.FirewallUpdates
            .Include(u => u.FirewallVersion)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (update == null)
        {
            return NotFound();
        }

        var model = new FirewallUpdateViewModel
        {
            Id = update.Id,
            FirewallVersionId = update.FirewallVersionId,
            FirewallVersionDisplay = update.FirewallVersion.DisplayName,
            ReleaseDate = update.ReleaseDate,
            FileName = update.FileName,
            FileSize = update.FileSize,
            FileHash = update.FileHash,
            ReleaseNotes = update.ReleaseNotes,
            IsActive = update.IsActive,
            IsCritical = update.IsCritical,
            MinimumFirewallVersionId = update.MinimumFirewallVersionId,
            AvailableFirewallVersions = await GetFirewallVersionSelectListAsync(),
            MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true)
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FirewallUpdateViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            model.MinimumFirewallVersions = await GetFirewallVersionSelectListAsync(includeNone: true);
            return View(model);
        }

        var update = await _context.FirewallUpdates
            .Include(u => u.FirewallVersion)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (update == null)
        {
            return NotFound();
        }

        if (model.UpdateFile != null)
        {
            var version = await _context.FirewallVersions.FindAsync(model.FirewallVersionId);
            if (version != null)
            {
                var (fileName, fileSize, fileHash) = await _fileStorageService.SaveFirewallUpdateAsync(model.UpdateFile, version.Version);
                update.FileName = fileName;
                update.FileSize = fileSize;
                update.FileHash = fileHash;
            }
        }

        update.FirewallVersionId = model.FirewallVersionId;
        update.ReleaseDate = model.ReleaseDate;
        update.ReleaseNotes = model.ReleaseNotes;
        update.IsActive = model.IsActive;
        update.IsCritical = model.IsCritical;
        update.MinimumFirewallVersionId = model.MinimumFirewallVersionId;
        update.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall update updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var update = await _context.FirewallUpdates.FindAsync(id);
        if (update == null)
        {
            return NotFound();
        }

        await _fileStorageService.DeleteFirewallUpdateAsync(update.FileName);
        _context.FirewallUpdates.Remove(update);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall update deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetFirewallVersionSelectListAsync(bool includeNone = false)
    {
        var versions = await _context.FirewallVersions
            .Where(v => v.IsActive)
            .OrderByDescending(v => v.ReleaseDate)
            .ToListAsync();

        var items = versions.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = v.DisplayName
        }).ToList();

        if (includeNone)
        {
            items.Insert(0, new SelectListItem { Value = "", Text = "-- None --" });
        }

        return items;
    }
}
