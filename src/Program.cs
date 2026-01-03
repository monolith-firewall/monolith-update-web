using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Data;
using MonolithUpdateSite.Middleware;
using MonolithUpdateSite.Models.Domain;
using MonolithUpdateSite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequiredMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var pending = await context.Database.GetPendingMigrationsAsync();
if (pending.Any())
    await context.Database.MigrateAsync();
else
    await context.Database.EnsureCreatedAsync();

        if (!context.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@monolith.com",
                Email = "admin@monolith.com",
                EmailConfirmed = true,
                RequirePasswordChange = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                Console.WriteLine("Default admin user created: admin@monolith.com / Admin123!");
            }
        }

        // Set RequirePasswordChange for all existing users
        var existingUsers = await context.Users.ToListAsync();
        bool anyUpdated = false;

        foreach (var existingUser in existingUsers)
        {
            if (!existingUser.RequirePasswordChange)
            {
                existingUser.RequirePasswordChange = true;
                anyUpdated = true;
            }
        }

        if (anyUpdated)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Updated {existingUsers.Count} existing user(s) to require password change.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
