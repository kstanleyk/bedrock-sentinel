using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "rbac");
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.Property(rp => rp.RemovedBy).HasMaxLength(256);
        builder.Property(rp => rp.TenantId).HasMaxLength(100);

        builder.HasIndex(rp => rp.RemovedOn);   // fast filter on active assignments
        builder.HasIndex(rp => rp.TenantId);

        builder.HasOne(rp => rp.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(rp => rp.RoleId);

        builder.HasOne(rp => rp.Permission)
               .WithMany(p => p.RolePermissions)
               .HasForeignKey(rp => rp.PermissionId);
    }
}
