using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonolithUpdateSite.Models.Domain;

namespace MonolithUpdateSite.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FirewallVersion> FirewallVersions { get; set; }
    public DbSet<FirewallUpdate> FirewallUpdates { get; set; }
    public DbSet<MonolithPackage> MonolithPackages { get; set; }
    public DbSet<PackageUpdate> PackageUpdates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.RequirePasswordChange).IsRequired();
            entity.HasIndex(e => e.RequirePasswordChange);

            entity.Property(e => e.TwoFactorSetupCompleted).IsRequired();
            entity.Property(e => e.RecoveryCodes).HasMaxLength(1000);
            entity.HasIndex(e => e.TwoFactorEnabled);
        });

        builder.Entity<FirewallVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Version).IsUnique();
        });

        builder.Entity<FirewallUpdate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(64);

            entity.HasOne(e => e.FirewallVersion)
                .WithMany(v => v.FirewallUpdates)
                .HasForeignKey(e => e.FirewallVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MinimumFirewallVersion)
                .WithMany()
                .HasForeignKey(e => e.MinimumFirewallVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MonolithPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PackageName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PackageCode).IsRequired().HasMaxLength(50);
            // Category is nullable to handle databases that haven't run the migration yet
            entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("Other");
            entity.HasIndex(e => e.PackageCode).IsUnique();
        });

        builder.Entity<PackageUpdate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => new { e.PackageId, e.Version });

            entity.HasOne(e => e.Package)
                .WithMany(p => p.PackageUpdates)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RequiredFirewallVersion)
                .WithMany(v => v.DependentPackageUpdates)
                .HasForeignKey(e => e.RequiredFirewallVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
