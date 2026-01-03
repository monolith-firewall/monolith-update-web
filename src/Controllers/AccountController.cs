using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;
using MonolithUpdateSite.Services;

namespace MonolithUpdateSite.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITwoFactorService twoFactorService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _twoFactorService = twoFactorService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2FA), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        }

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/Admin/Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new ChangePasswordViewModel
        {
            IsRequired = user.RequirePasswordChange
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        model.IsRequired = user.RequirePasswordChange;

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            user.RequirePasswordChange = false;
            user.LastPasswordChangedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Your password has been changed successfully.";

            if (user.LastPasswordChangedAt == DateTime.UtcNow && !user.TwoFactorEnabled)
            {
                TempData["Info"] = "Would you like to enhance your security with Two-Factor Authentication?";
                return RedirectToAction(nameof(Setup2FA));
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Setup2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (user.TwoFactorEnabled)
        {
            return RedirectToAction(nameof(Manage2FA));
        }

        var model = await _twoFactorService.GenerateSetupInfo(user);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Setup2FA(Enable2FAViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var setupInfo = await _twoFactorService.GenerateSetupInfo(user);
                model.SharedKey = setupInfo.SharedKey;
                model.AuthenticatorUri = setupInfo.AuthenticatorUri;
                model.QrCodeDataUrl = setupInfo.QrCodeDataUrl;
            }
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var isCodeValid = await _twoFactorService.VerifyTwoFactorCode(currentUser, model.Code);

        if (!isCodeValid)
        {
            ModelState.AddModelError(nameof(model.Code), "Verification code is invalid.");
            var setupInfo = await _twoFactorService.GenerateSetupInfo(currentUser);
            model.SharedKey = setupInfo.SharedKey;
            model.AuthenticatorUri = setupInfo.AuthenticatorUri;
            model.QrCodeDataUrl = setupInfo.QrCodeDataUrl;
            return View(model);
        }

        await _userManager.SetTwoFactorEnabledAsync(currentUser, true);
        currentUser.TwoFactorSetupCompleted = true;
        currentUser.TwoFactorEnabledAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(currentUser);

        var recoveryCodes = await _twoFactorService.GenerateRecoveryCodes(currentUser);
        TempData["RecoveryCodes"] = System.Text.Json.JsonSerializer.Serialize(recoveryCodes);

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ShowRecoveryCodes()
    {
        var recoveryCodesJson = TempData["RecoveryCodes"] as string;
        if (string.IsNullOrEmpty(recoveryCodesJson))
        {
            return RedirectToAction(nameof(Manage2FA));
        }

        var recoveryCodes = System.Text.Json.JsonSerializer.Deserialize<string[]>(recoveryCodesJson) ?? Array.Empty<string>();
        var model = new RecoveryCodesViewModel { RecoveryCodes = recoveryCodes };

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Manage2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new Manage2FAViewModel
        {
            Is2FAEnabled = user.TwoFactorEnabled,
            EnabledAt = user.TwoFactorEnabledAt,
            RemainingRecoveryCodes = await _twoFactorService.CountValidRecoveryCodes(user)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Disable2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        user.TwoFactorSetupCompleted = false;
        user.TwoFactorEnabledAt = null;
        user.RecoveryCodes = null;
        await _userManager.UpdateAsync(user);

        await _userManager.ResetAuthenticatorKeyAsync(user);

        TempData["Success"] = "Two-Factor Authentication has been disabled.";
        return RedirectToAction(nameof(Manage2FA));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> RegenerateRecoveryCodes()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!user.TwoFactorEnabled)
        {
            return RedirectToAction(nameof(Manage2FA));
        }

        var recoveryCodes = await _twoFactorService.GenerateRecoveryCodes(user);
        TempData["RecoveryCodes"] = System.Text.Json.JsonSerializer.Serialize(recoveryCodes);
        TempData["Success"] = "New recovery codes have been generated.";

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [HttpGet]
    public IActionResult LoginWith2FA(string? returnUrl = null, bool rememberMe = false)
    {
        var model = new Verify2FAViewModel
        {
            ReturnUrl = returnUrl,
            RememberMachine = rememberMe
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2FA(Verify2FAViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to load two-factor authentication user.");
            return View(model);
        }

        var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMachine, rememberClient: model.RememberMachine);

        if (result.Succeeded)
        {
            return LocalRedirect(model.ReturnUrl ?? "/Admin/Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked out.");
            return View(model);
        }

        var recoveryCodeResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(authenticatorCode);
        if (recoveryCodeResult.Succeeded)
        {
            TempData["Warning"] = "You have used a recovery code to login. Please regenerate new recovery codes.";
            return LocalRedirect(model.ReturnUrl ?? "/Admin/Dashboard");
        }

        ModelState.AddModelError(nameof(model.TwoFactorCode), "Invalid authenticator code or recovery code.");
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new EditProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            UserName = user.UserName ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        bool needsReSignIn = false;

        // Update email if changed
        if (model.Email != user.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null && emailExists.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already in use.");
                return View(model);
            }

            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpperInvariant();
            user.UserName = model.Email;
            user.NormalizedUserName = model.Email.ToUpperInvariant();
            user.EmailConfirmed = true;
            needsReSignIn = true;
        }

        // Update phone number
        if (model.PhoneNumber != user.PhoneNumber)
        {
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = !string.IsNullOrEmpty(model.PhoneNumber);
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = "Profile updated successfully!";

        // Re-sign in the user if email was changed
        if (needsReSignIn)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        return RedirectToAction(nameof(EditProfile));
    }
}
