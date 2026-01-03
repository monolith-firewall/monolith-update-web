using System.Security.Cryptography;

namespace MonolithUpdateSite.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _firewallUpdatesPath;
    private readonly string _packageUpdatesPath;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _firewallUpdatesPath = Path.Combine(_environment.WebRootPath, "updates", "firewall");
        _packageUpdatesPath = Path.Combine(_environment.WebRootPath, "updates", "packages");

        Directory.CreateDirectory(_firewallUpdatesPath);
        Directory.CreateDirectory(_packageUpdatesPath);
    }

    public async Task<(string fileName, long fileSize, string fileHash)> SaveFirewallUpdateAsync(IFormFile file, string version)
    {
        var versionFolder = Path.Combine(_firewallUpdatesPath, version);
        Directory.CreateDirectory(versionFolder);

        var fileName = file.FileName;
        var filePath = Path.Combine(versionFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            var fileHash = await CalculateFileHashAsync(stream);
            var fileSize = stream.Length;
            return (fileName, fileSize, fileHash);
        }
    }

    public async Task<(string fileName, long fileSize, string fileHash)> SavePackageUpdateAsync(IFormFile file, string packageCode, string version)
    {
        var packageFolder = Path.Combine(_packageUpdatesPath, packageCode.ToLower());
        var versionFolder = Path.Combine(packageFolder, version);
        Directory.CreateDirectory(versionFolder);

        var fileName = file.FileName;
        var filePath = Path.Combine(versionFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            var fileHash = await CalculateFileHashAsync(stream);
            var fileSize = stream.Length;
            return (fileName, fileSize, fileHash);
        }
    }

    public Task<bool> DeleteFirewallUpdateAsync(string fileName)
    {
        try
        {
            var filePath = GetFirewallUpdatePath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeletePackageUpdateAsync(string packageCode, string fileName)
    {
        try
        {
            var filePath = GetPackageUpdatePath(packageCode, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GetFirewallUpdatePath(string fileName)
    {
        var files = Directory.GetFiles(_firewallUpdatesPath, fileName, SearchOption.AllDirectories);
        return files.FirstOrDefault() ?? string.Empty;
    }

    public string GetPackageUpdatePath(string packageCode, string fileName)
    {
        var packageFolder = Path.Combine(_packageUpdatesPath, packageCode.ToLower());
        if (!Directory.Exists(packageFolder))
            return string.Empty;

        var files = Directory.GetFiles(packageFolder, fileName, SearchOption.AllDirectories);
        return files.FirstOrDefault() ?? string.Empty;
    }

    public async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(fileStream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
