using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MonolithUpdateSite.Models.ViewModels;

public class PackageUpdateViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Package")]
    public int PackageId { get; set; }

    public string? PackageName { get; set; }
    public List<SelectListItem>? AvailablePackages { get; set; }

    [Required]
    [Display(Name = "Version")]
    public string Version { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;

    [Display(Name = "Package File")]
    public IFormFile? PackageFile { get; set; }

    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? FileHash { get; set; }

    [Display(Name = "Release Notes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Critical Update")]
    public bool IsCritical { get; set; }

    [Display(Name = "Minimum Package Version")]
    public string? MinimumPackageVersion { get; set; }

    [Display(Name = "Required Firewall Version")]
    public int? RequiredFirewallVersionId { get; set; }

    public List<SelectListItem>? AvailableFirewallVersions { get; set; }
}
