using System.ComponentModel.DataAnnotations;

namespace MonolithUpdateSite.Models.ViewModels;

public class FirewallVersionViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Version")]
    public string Version { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}
