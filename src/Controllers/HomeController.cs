using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.ViewModels;

namespace MonolithUpdateSite.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var firewallVersions = await _context.FirewallVersions
            .Where(v => v.IsActive)
            .Include(v => v.FirewallUpdates.Where(u => u.IsActive))
            .OrderByDescending(v => v.ReleaseDate)
            .ToListAsync();

        var packages = await _context.MonolithPackages
            .Where(p => p.IsActive)
            .Include(p => p.PackageUpdates.Where(u => u.IsActive))
            .OrderBy(p => p.PackageName)
            .ToListAsync();

        var viewModel = new PublicHomeViewModel
        {
            FirewallVersions = firewallVersions,
            Packages = packages
        };

        return View(viewModel);
    }
}
