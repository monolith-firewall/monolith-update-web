using MonolithUpdateSite.Models.Domain;

namespace MonolithUpdateSite.Models.ViewModels;

public class PublicHomeViewModel
{
    public List<FirewallVersion> FirewallVersions { get; set; } = new();
    public List<MonolithPackage> Packages { get; set; } = new();
}
