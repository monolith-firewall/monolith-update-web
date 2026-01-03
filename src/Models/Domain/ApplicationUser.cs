using Microsoft.AspNetCore.Identity;

namespace MonolithUpdateSite.Models.Domain;

public class ApplicationUser : IdentityUser
{
    public bool RequirePasswordChange { get; set; } = false;
    public DateTime? LastPasswordChangedAt { get; set; }

    public bool TwoFactorSetupCompleted { get; set; } = false;
    public DateTime? TwoFactorEnabledAt { get; set; }
    public string? RecoveryCodes { get; set; }
}
