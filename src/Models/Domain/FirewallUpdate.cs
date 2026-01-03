namespace MonolithUpdateSite.Models.Domain;

public class FirewallUpdate
{
    public int Id { get; set; }
    public int FirewallVersionId { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsCritical { get; set; }
    public int? MinimumFirewallVersionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public FirewallVersion FirewallVersion { get; set; } = null!;
    public FirewallVersion? MinimumFirewallVersion { get; set; }
}
