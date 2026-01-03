using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;

namespace MonolithUpdateSite.Services;

public interface ITwoFactorService
{
    Task<Enable2FAViewModel> GenerateSetupInfo(ApplicationUser user);
    Task<bool> VerifyTwoFactorCode(ApplicationUser user, string code);
    Task<string[]> GenerateRecoveryCodes(ApplicationUser user);
    Task<bool> ValidateRecoveryCode(ApplicationUser user, string code);
    Task<int> CountValidRecoveryCodes(ApplicationUser user);
}
