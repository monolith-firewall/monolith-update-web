using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;

namespace MonolithUpdateSite.Controllers.Admin;

[Authorize]
[Route("Admin/[controller]")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var firewallVersionsCount = await _context.FirewallVersions.CountAsync(v => v.IsActive);
        var firewallUpdatesCount = await _context.FirewallUpdates.CountAsync(u => u.IsActive);
        var packagesCount = await _context.MonolithPackages.CountAsync(p => p.IsActive);
        var packageUpdatesCount = await _context.PackageUpdates.CountAsync(u => u.IsActive);

        var recentFirewallUpdates = await _context.FirewallUpdates
            .Include(u => u.FirewallVersion)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentPackageUpdates = await _context.PackageUpdates
            .Include(u => u.Package)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.FirewallVersionsCount = firewallVersionsCount;
        ViewBag.FirewallUpdatesCount = firewallUpdatesCount;
        ViewBag.PackagesCount = packagesCount;
        ViewBag.PackageUpdatesCount = packageUpdatesCount;
        ViewBag.RecentFirewallUpdates = recentFirewallUpdates;
        ViewBag.RecentPackageUpdates = recentPackageUpdates;

        return View();
    }
}
