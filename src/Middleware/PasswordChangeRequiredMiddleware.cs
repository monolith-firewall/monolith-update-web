using Microsoft.AspNetCore.Identity;
using MonolithUpdateSite.Models.Domain;

namespace MonolithUpdateSite.Middleware;

public class PasswordChangeRequiredMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeRequiredMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user?.RequirePasswordChange == true)
            {
                var allowedPaths = new[]
                {
                    "/Account/ChangePassword",
                    "/Account/Logout",
                    "/Account/Setup2FA",
                    "/Account/ShowRecoveryCodes",
                    "/Account/LoginWith2FA",
                    "/css/",
                    "/js/",
                    "/lib/"
                };

                var path = context.Request.Path.Value ?? "";
                var isAllowedPath = allowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                if (!isAllowedPath)
                {
                    context.Response.Redirect("/Account/ChangePassword");
                    return;
                }
            }
        }

        await _next(context);
    }
}
