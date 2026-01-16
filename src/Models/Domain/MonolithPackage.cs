namespace MonolithUpdateSite.Models.Domain;

public class MonolithPackage
{
    public int Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; } = "Other";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PackageUpdate> PackageUpdates { get; set; } = new List<PackageUpdate>();
}
