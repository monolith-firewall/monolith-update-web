namespace MonolithUpdateSite.Models.Domain;

public class PackageUpdate
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsCritical { get; set; }
    public string? MinimumPackageVersion { get; set; }
    public int? RequiredFirewallVersionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public MonolithPackage Package { get; set; } = null!;
    public FirewallVersion? RequiredFirewallVersion { get; set; }
}
