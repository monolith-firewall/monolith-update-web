namespace MonolithUpdateSite.Services;

public interface IFileStorageService
{
    Task<(string fileName, long fileSize, string fileHash)> SaveFirewallUpdateAsync(IFormFile file, string version);
    Task<(string fileName, long fileSize, string fileHash)> SavePackageUpdateAsync(IFormFile file, string packageCode, string version);
    Task<bool> DeleteFirewallUpdateAsync(string fileName);
    Task<bool> DeletePackageUpdateAsync(string packageCode, string fileName);
    string GetFirewallUpdatePath(string fileName);
    string GetPackageUpdatePath(string packageCode, string fileName);
    Task<string> CalculateFileHashAsync(Stream fileStream);
}
