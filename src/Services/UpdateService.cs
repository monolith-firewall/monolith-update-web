using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Models.ApiResponses;

namespace MonolithUpdateSite.Services;

public class UpdateService : IUpdateService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public FirewallUpdateResponse CheckFirewallUpdate(string currentVersion)
    {
        var latestUpdate = _context.FirewallUpdates
            .Include(u => u.FirewallVersion)
            .Include(u => u.MinimumFirewallVersion)
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.FirewallVersion.ReleaseDate)
            .FirstOrDefault();

        if (latestUpdate == null)
        {
            return new FirewallUpdateResponse
            {
                UpdateAvailable = false,
                CurrentVersion = currentVersion,
                Message = "No updates available at this time."
            };
        }

        var updateAvailable = CompareVersions(latestUpdate.FirewallVersion.Version, currentVersion);

        if (!updateAvailable)
        {
            return new FirewallUpdateResponse
            {
                UpdateAvailable = false,
                LatestVersion = latestUpdate.FirewallVersion.Version,
                CurrentVersion = currentVersion,
                Message = "You are running the latest version."
            };
        }

        if (latestUpdate.MinimumFirewallVersion != null)
        {
            if (!CompareVersions(currentVersion, latestUpdate.MinimumFirewallVersion.Version) &&
                currentVersion != latestUpdate.MinimumFirewallVersion.Version)
            {
                return new FirewallUpdateResponse
                {
                    UpdateAvailable = false,
                    LatestVersion = latestUpdate.FirewallVersion.Version,
                    CurrentVersion = currentVersion,
                    Message = $"Update requires minimum version {latestUpdate.MinimumFirewallVersion.Version}."
                };
            }
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var downloadUrl = $"{request?.Scheme}://{request?.Host}/api/v1/firewall/download/{latestUpdate.FirewallVersion.Version}";

        return new FirewallUpdateResponse
        {
            UpdateAvailable = true,
            LatestVersion = latestUpdate.FirewallVersion.Version,
            CurrentVersion = currentVersion,
            ReleaseDate = latestUpdate.ReleaseDate,
            DownloadUrl = downloadUrl,
            FileSize = latestUpdate.FileSize,
            FileHash = latestUpdate.FileHash,
            IsCritical = latestUpdate.IsCritical,
            ReleaseNotes = latestUpdate.ReleaseNotes,
            MinimumCurrentVersion = latestUpdate.MinimumFirewallVersion?.Version,
            Message = latestUpdate.IsCritical ? "Critical update available!" : "New update available."
        };
    }

    public PackageUpdateResponse CheckPackageUpdate(string packageCode, string currentVersion, string? firewallVersion = null)
    {
        var package = _context.MonolithPackages
            .Include(p => p.PackageUpdates)
                .ThenInclude(u => u.RequiredFirewallVersion)
            .FirstOrDefault(p => p.PackageCode == packageCode && p.IsActive);

        if (package == null)
        {
            return new PackageUpdateResponse
            {
                UpdateAvailable = false,
                PackageCode = packageCode,
                PackageName = "Unknown",
                CurrentVersion = currentVersion,
                Message = "Package not found."
            };
        }

        var latestUpdate = package.PackageUpdates
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.ReleaseDate)
            .FirstOrDefault();

        if (latestUpdate == null)
        {
            return new PackageUpdateResponse
            {
                UpdateAvailable = false,
                PackageCode = packageCode,
                PackageName = package.PackageName,
                CurrentVersion = currentVersion,
                Message = "No updates available for this package."
            };
        }

        var updateAvailable = CompareVersions(latestUpdate.Version, currentVersion);

        if (!updateAvailable)
        {
            return new PackageUpdateResponse
            {
                UpdateAvailable = false,
                PackageCode = packageCode,
                PackageName = package.PackageName,
                LatestVersion = latestUpdate.Version,
                CurrentVersion = currentVersion,
                Message = "You are running the latest version."
            };
        }

        if (latestUpdate.RequiredFirewallVersion != null && !string.IsNullOrEmpty(firewallVersion))
        {
            if (!CompareVersions(firewallVersion, latestUpdate.RequiredFirewallVersion.Version) &&
                firewallVersion != latestUpdate.RequiredFirewallVersion.Version)
            {
                return new PackageUpdateResponse
                {
                    UpdateAvailable = false,
                    PackageCode = packageCode,
                    PackageName = package.PackageName,
                    LatestVersion = latestUpdate.Version,
                    CurrentVersion = currentVersion,
                    Message = $"Update requires firewall version {latestUpdate.RequiredFirewallVersion.Version} or higher."
                };
            }
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var downloadUrl = $"{request?.Scheme}://{request?.Host}/api/v1/packages/download/{packageCode}/{latestUpdate.Version}";

        return new PackageUpdateResponse
        {
            UpdateAvailable = true,
            PackageCode = packageCode,
            PackageName = package.PackageName,
            LatestVersion = latestUpdate.Version,
            CurrentVersion = currentVersion,
            ReleaseDate = latestUpdate.ReleaseDate,
            DownloadUrl = downloadUrl,
            FileSize = latestUpdate.FileSize,
            FileHash = latestUpdate.FileHash,
            IsCritical = latestUpdate.IsCritical,
            ReleaseNotes = latestUpdate.ReleaseNotes,
            RequiredFirewallVersion = latestUpdate.RequiredFirewallVersion?.Version,
            Message = latestUpdate.IsCritical ? "Critical update available!" : "New update available."
        };
    }

    public PackageListResponse GetAvailablePackages(string? firewallVersion = null)
    {
        var packages = _context.MonolithPackages
            .Include(p => p.PackageUpdates)
                .ThenInclude(u => u.RequiredFirewallVersion)
            .Where(p => p.IsActive)
            .ToList();

        var packageInfos = new List<PackageInfo>();

        foreach (var package in packages)
        {
            var latestUpdate = package.PackageUpdates
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.ReleaseDate)
                .FirstOrDefault();

            if (latestUpdate != null)
            {
                bool includePackage = true;

                if (!string.IsNullOrEmpty(firewallVersion) && latestUpdate.RequiredFirewallVersion != null)
                {
                    includePackage = CompareVersions(firewallVersion, latestUpdate.RequiredFirewallVersion.Version) ||
                                   firewallVersion == latestUpdate.RequiredFirewallVersion.Version;
                }

                if (includePackage)
                {
                    packageInfos.Add(new PackageInfo
                    {
                        PackageCode = package.PackageCode,
                        PackageName = package.PackageName,
                        Description = package.Description,
                        LatestVersion = latestUpdate.Version,
                        ReleaseDate = latestUpdate.ReleaseDate
                    });
                }
            }
        }

        return new PackageListResponse
        {
            Packages = packageInfos
        };
    }

    public bool CompareVersions(string version1, string version2)
    {
        try
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1 > v2;
        }
        catch
        {
            return false;
        }
    }
}
