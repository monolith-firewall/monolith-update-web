using Microsoft.AspNetCore.Identity;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Models.ViewModels;
using QRCoder;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MonolithUpdateSite.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UrlEncoder _urlEncoder;

    public TwoFactorService(UserManager<ApplicationUser> userManager, UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
    }

    public async Task<Enable2FAViewModel> GenerateSetupInfo(ApplicationUser user)
    {
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var sharedKey = FormatKey(unformattedKey!);
        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        var qrCodeDataUrl = GenerateQrCode(authenticatorUri);

        return new Enable2FAViewModel
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeDataUrl = qrCodeDataUrl
        };
    }

    public async Task<bool> VerifyTwoFactorCode(ApplicationUser user, string code)
    {
        var cleanCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            cleanCode);

        return isCodeValid;
    }

    public async Task<string[]> GenerateRecoveryCodes(ApplicationUser user)
    {
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        if (recoveryCodes != null)
        {
            user.RecoveryCodes = JsonSerializer.Serialize(recoveryCodes);
            await _userManager.UpdateAsync(user);
            return recoveryCodes.ToArray();
        }

        return Array.Empty<string>();
    }

    public async Task<bool> ValidateRecoveryCode(ApplicationUser user, string code)
    {
        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
        return result.Succeeded;
    }

    public async Task<int> CountValidRecoveryCodes(ApplicationUser user)
    {
        var count = await _userManager.CountRecoveryCodesAsync(user);
        return count;
    }

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("MonolithUpdateSite"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    private string GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);

        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
    }
}
