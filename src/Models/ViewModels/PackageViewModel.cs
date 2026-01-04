using System.ComponentModel.DataAnnotations;

namespace MonolithUpdateSite.Models.ViewModels;

public class PackageViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Package Name")]
    public string PackageName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Package Code")]
    public string PackageCode { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public string Category { get; set; } = "Other";

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}
