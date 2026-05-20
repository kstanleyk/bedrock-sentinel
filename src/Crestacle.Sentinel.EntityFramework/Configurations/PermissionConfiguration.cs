using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "rbac");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Feature).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Group).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(300).IsRequired();
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasIndex(p => new { p.Feature, p.Action }).IsUnique();

        builder.HasMany(p => p.RolePermissions)
               .WithOne(rp => rp.Permission)
               .HasForeignKey(rp => rp.PermissionId);
    }
}
