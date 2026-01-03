using System.ComponentModel.DataAnnotations;

namespace MonolithUpdateSite.Models.ViewModels;

public class Verify2FAViewModel
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [Display(Name = "Authenticator Code")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Remember this device")]
    public bool RememberMachine { get; set; }

    public string? ReturnUrl { get; set; }
}
