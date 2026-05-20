using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "rbac");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.IdentityId).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(300);
        builder.Property(u => u.FullName).HasMaxLength(200);
        builder.Property(u => u.Phone).HasMaxLength(50);
        builder.Property(u => u.TenantId).HasMaxLength(100);

        builder.HasIndex(u => u.IdentityId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.TenantId);

        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId);
    }
}
