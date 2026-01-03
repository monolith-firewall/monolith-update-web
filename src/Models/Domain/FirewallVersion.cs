namespace MonolithUpdateSite.Models.Domain;

public class FirewallVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FirewallUpdate> FirewallUpdates { get; set; } = new List<FirewallUpdate>();
    public ICollection<PackageUpdate> DependentPackageUpdates { get; set; } = new List<PackageUpdate>();
}
