namespace MonolithUpdateSite.Models.ApiResponses;

public class PackageListResponse
{
    public List<PackageInfo> Packages { get; set; } = new();
}

public class PackageInfo
{
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
}
