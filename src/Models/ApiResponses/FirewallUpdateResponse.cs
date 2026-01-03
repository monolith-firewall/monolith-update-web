namespace MonolithUpdateSite.Models.ApiResponses;

public class FirewallUpdateResponse
{
    public bool UpdateAvailable { get; set; }
    public string? LatestVersion { get; set; }
    public string? CurrentVersion { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? DownloadUrl { get; set; }
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public bool IsCritical { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? MinimumCurrentVersion { get; set; }
    public string Message { get; set; } = string.Empty;
}
