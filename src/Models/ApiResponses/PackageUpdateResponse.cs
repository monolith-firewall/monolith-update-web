namespace MonolithUpdateSite.Models.ApiResponses;

public class PackageUpdateResponse
{
    public bool UpdateAvailable { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string? LatestVersion { get; set; }
    public string? CurrentVersion { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? DownloadUrl { get; set; }
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public bool IsCritical { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? RequiredFirewallVersion { get; set; }
    public string Message { get; set; } = string.Empty;
}
