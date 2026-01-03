using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;
using MonolithUpdateSite.Services;

namespace MonolithUpdateSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PackageUpdatesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public PackageUpdatesController(ApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int? packageId)
    {
        var query = _context.PackageUpdates
            .Include(u => u.Package)
            .Include(u => u.RequiredFirewallVersion)
            .AsQueryable();

        if (packageId.HasValue)
        {
            query = query.Where(u => u.PackageId == packageId.Value);
        }

        var updates = await query
            .OrderByDescending(u => u.ReleaseDate)
            .ToListAsync();

        ViewBag.Packages = await _context.MonolithPackages.ToListAsync();
        ViewBag.SelectedPackageId = packageId;

        return View(updates);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new PackageUpdateViewModel
        {
            AvailablePackages = await GetPackageSelectListAsync(),
            AvailableFirewallVersions = await GetFirewallVersionSelectListAsync()
        };
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PackageUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailablePackages = await GetPackageSelectListAsync();
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            return View(model);
        }

        if (model.PackageFile == null)
        {
            ModelState.AddModelError("PackageFile", "Package file is required.");
            model.AvailablePackages = await GetPackageSelectListAsync();
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            return View(model);
        }

        var package = await _context.MonolithPackages.FindAsync(model.PackageId);
        if (package == null)
        {
            ModelState.AddModelError("PackageId", "Invalid package selected.");
            model.AvailablePackages = await GetPackageSelectListAsync();
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            return View(model);
        }

        var (fileName, fileSize, fileHash) = await _fileStorageService.SavePackageUpdateAsync(
            model.PackageFile, package.PackageCode, model.Version);

        var update = new PackageUpdate
        {
            PackageId = model.PackageId,
            Version = model.Version,
            ReleaseDate = model.ReleaseDate,
            FileName = fileName,
            FileSize = fileSize,
            FileHash = fileHash,
            ReleaseNotes = model.ReleaseNotes,
            IsActive = model.IsActive,
            IsCritical = model.IsCritical,
            MinimumPackageVersion = model.MinimumPackageVersion,
            RequiredFirewallVersionId = model.RequiredFirewallVersionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PackageUpdates.Add(update);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Package update created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var update = await _context.PackageUpdates
            .Include(u => u.Package)
            .Include(u => u.RequiredFirewallVersion)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (update == null)
        {
            return NotFound();
        }

        var model = new PackageUpdateViewModel
        {
            Id = update.Id,
            PackageId = update.PackageId,
            PackageName = update.Package.PackageName,
            Version = update.Version,
            ReleaseDate = update.ReleaseDate,
            FileName = update.FileName,
            FileSize = update.FileSize,
            FileHash = update.FileHash,
            ReleaseNotes = update.ReleaseNotes,
            IsActive = update.IsActive,
            IsCritical = update.IsCritical,
            MinimumPackageVersion = update.MinimumPackageVersion,
            RequiredFirewallVersionId = update.RequiredFirewallVersionId,
            AvailablePackages = await GetPackageSelectListAsync(),
            AvailableFirewallVersions = await GetFirewallVersionSelectListAsync()
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PackageUpdateViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.AvailablePackages = await GetPackageSelectListAsync();
            model.AvailableFirewallVersions = await GetFirewallVersionSelectListAsync();
            return View(model);
        }

        var update = await _context.PackageUpdates
            .Include(u => u.Package)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (update == null)
        {
            return NotFound();
        }

        if (model.PackageFile != null)
        {
            var (fileName, fileSize, fileHash) = await _fileStorageService.SavePackageUpdateAsync(
                model.PackageFile, update.Package.PackageCode, model.Version);

            update.FileName = fileName;
            update.FileSize = fileSize;
            update.FileHash = fileHash;
        }

        update.PackageId = model.PackageId;
        update.Version = model.Version;
        update.ReleaseDate = model.ReleaseDate;
        update.ReleaseNotes = model.ReleaseNotes;
        update.IsActive = model.IsActive;
        update.IsCritical = model.IsCritical;
        update.MinimumPackageVersion = model.MinimumPackageVersion;
        update.RequiredFirewallVersionId = model.RequiredFirewallVersionId;
        update.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Package update updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var update = await _context.PackageUpdates
            .Include(u => u.Package)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (update == null)
        {
            return NotFound();
        }

        await _fileStorageService.DeletePackageUpdateAsync(update.Package.PackageCode, update.FileName);
        _context.PackageUpdates.Remove(update);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Package update deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetPackageSelectListAsync()
    {
        var packages = await _context.MonolithPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.PackageName)
            .ToListAsync();

        return packages.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.PackageName} ({p.PackageCode})"
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetFirewallVersionSelectListAsync()
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

        items.Insert(0, new SelectListItem { Value = "", Text = "-- None (Any Version) --" });

        return items;
    }
}
