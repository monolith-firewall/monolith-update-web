using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;

namespace MonolithUpdateSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class FirewallVersionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FirewallVersionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var versions = await _context.FirewallVersions
            .OrderByDescending(v => v.ReleaseDate)
            .ToListAsync();
        return View(versions);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new FirewallVersionViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FirewallVersionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingVersion = await _context.FirewallVersions
            .FirstOrDefaultAsync(v => v.Version == model.Version);

        if (existingVersion != null)
        {
            ModelState.AddModelError("Version", "This version already exists.");
            return View(model);
        }

        var version = new FirewallVersion
        {
            Version = model.Version,
            DisplayName = model.DisplayName,
            ReleaseDate = model.ReleaseDate,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.FirewallVersions.Add(version);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall version created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var version = await _context.FirewallVersions.FindAsync(id);
        if (version == null)
        {
            return NotFound();
        }

        var model = new FirewallVersionViewModel
        {
            Id = version.Id,
            Version = version.Version,
            DisplayName = version.DisplayName,
            ReleaseDate = version.ReleaseDate,
            IsActive = version.IsActive
        };

        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FirewallVersionViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var version = await _context.FirewallVersions.FindAsync(id);
        if (version == null)
        {
            return NotFound();
        }

        version.Version = model.Version;
        version.DisplayName = model.DisplayName;
        version.ReleaseDate = model.ReleaseDate;
        version.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall version updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var version = await _context.FirewallVersions
            .Include(v => v.FirewallUpdates)
            .Include(v => v.DependentPackageUpdates)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (version == null)
        {
            return NotFound();
        }

        if (version.FirewallUpdates.Any() || version.DependentPackageUpdates.Any())
        {
            TempData["Error"] = "Cannot delete version that is referenced by updates or packages.";
            return RedirectToAction(nameof(Index));
        }

        _context.FirewallVersions.Remove(version);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Firewall version deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
