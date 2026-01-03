using MonolithUpdateSite.Models.ApiResponses;
using MonolithUpdateSite.Models.Domain;

namespace MonolithUpdateSite.Services;

public interface IUpdateService
{
    FirewallUpdateResponse CheckFirewallUpdate(string currentVersion);
    PackageUpdateResponse CheckPackageUpdate(string packageCode, string currentVersion, string? firewallVersion = null);
    PackageListResponse GetAvailablePackages(string? firewallVersion = null);
    bool CompareVersions(string version1, string version2);
}
