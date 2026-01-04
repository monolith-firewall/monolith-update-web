using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;

namespace MonolithUpdateSite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PackagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PackagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var packages = await _context.MonolithPackages
            .OrderBy(p => p.PackageName)
            .ToListAsync();
        return View(packages);
    }

    public IActionResult Create()
    {
        return View(new PackageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PackageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingPackage = await _context.MonolithPackages
            .FirstOrDefaultAsync(p => p.PackageCode == model.PackageCode);

        if (existingPackage != null)
        {
            ModelState.AddModelError("PackageCode", "A package with this code already exists.");
            return View(model);
        }

        var package = new MonolithPackage
        {
            PackageName = model.PackageName,
            PackageCode = model.PackageCode,
            Description = model.Description,
            Category = string.IsNullOrWhiteSpace(model.Category) ? "Other" : model.Category.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.MonolithPackages.Add(package);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Package created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var package = await _context.MonolithPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        var model = new PackageViewModel
        {
            Id = package.Id,
            PackageName = package.PackageName,
            PackageCode = package.PackageCode,
            Description = package.Description,
            Category = package.Category,
            IsActive = package.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PackageViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var package = await _context.MonolithPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        package.PackageName = model.PackageName;
        package.PackageCode = model.PackageCode;
        package.Description = model.Description;
        package.Category = string.IsNullOrWhiteSpace(model.Category) ? "Other" : model.Category.Trim();
        package.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Package updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.MonolithPackages
            .Include(p => p.PackageUpdates)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
        {
            return NotFound();
        }

        _context.MonolithPackages.Remove(package);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Package deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
