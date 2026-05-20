using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "rbac");
        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.RemovedBy).HasMaxLength(256);
        builder.Property(ur => ur.TenantId).HasMaxLength(100);

        // Unique constraint: only one active (non-removed) assignment per (user, role, tenant).
        // Partial uniqueness is enforced in application logic since most providers don't support
        // filtered unique indexes via EF Core out of the box.
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.TenantId });
        builder.HasIndex(ur => ur.RemovedOn);   // fast filter on active assignments
        builder.HasIndex(ur => ur.ExpiresOn);   // fast filter on non-expired assignments
        builder.HasIndex(ur => ur.TenantId);

        builder.HasOne(ur => ur.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(ur => ur.UserId);

        builder.HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId);
    }
}
