using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MonolithUpdateSite.Models.ViewModels;

public class FirewallUpdateViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Firewall Version")]
    public int FirewallVersionId { get; set; }

    public string? FirewallVersionDisplay { get; set; }
    public List<SelectListItem>? AvailableFirewallVersions { get; set; }

    [Required]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;

    [Display(Name = "Update File")]
    public IFormFile? UpdateFile { get; set; }

    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? FileHash { get; set; }

    [Display(Name = "Release Notes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Critical Update")]
    public bool IsCritical { get; set; }

    [Display(Name = "Minimum Firewall Version")]
    public int? MinimumFirewallVersionId { get; set; }

    public List<SelectListItem>? MinimumFirewallVersions { get; set; }
}
