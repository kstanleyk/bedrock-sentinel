using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "rbac");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.DisplayName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.TenantId).HasMaxLength(100);
        builder.Property(r => r.RowVersion).IsRowVersion();

        // Role name is unique per tenant (null = global).
        builder.HasIndex(r => new { r.Name, r.TenantId }).IsUnique();
        builder.HasIndex(r => r.TenantId);

        // Self-referential FK for role hierarchy.
        builder.HasOne(r => r.ParentRole)
               .WithMany(r => r.ChildRoles)
               .HasForeignKey(r => r.ParentRoleId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.UserRoles)
               .WithOne(ur => ur.Role)
               .HasForeignKey(ur => ur.RoleId);

        builder.HasMany(r => r.RolePermissions)
               .WithOne(rp => rp.Role)
               .HasForeignKey(rp => rp.RoleId);
    }
}
