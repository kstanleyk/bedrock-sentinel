using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class PermissionConflictConfiguration : IEntityTypeConfiguration<PermissionConflict>
{
    public void Configure(EntityTypeBuilder<PermissionConflict> builder)
    {
        builder.ToTable("permission_conflicts", "rbac");
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.PermissionIdA).HasMaxLength(150).IsRequired();
        builder.Property(pc => pc.PermissionIdB).HasMaxLength(150).IsRequired();
        builder.Property(pc => pc.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(pc => pc.TenantId).HasMaxLength(100);

        // Prevent duplicate conflict pairs per tenant.
        builder.HasIndex(pc => new { pc.PermissionIdA, pc.PermissionIdB, pc.TenantId }).IsUnique();
    }
}
