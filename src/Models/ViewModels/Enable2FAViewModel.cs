using System.ComponentModel.DataAnnotations;

namespace MonolithUpdateSite.Models.ViewModels;

public class Enable2FAViewModel
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    public string QrCodeDataUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;
}
