using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Sentinel.EntityFramework.Configurations;

internal sealed class PendingAssignmentConfiguration : IEntityTypeConfiguration<PendingAssignment>
{
    public void Configure(EntityTypeBuilder<PendingAssignment> builder)
    {
        builder.ToTable("pending_assignments", "rbac");
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.RequestedBy).HasMaxLength(256).IsRequired();
        builder.Property(pa => pa.ReviewedBy).HasMaxLength(256);
        builder.Property(pa => pa.RejectionReason).HasMaxLength(500);
        builder.Property(pa => pa.RoleExpiresOn);
        builder.Property(pa => pa.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(pa => pa.TenantId).HasMaxLength(100);
        builder.Property(pa => pa.RowVersion).IsRowVersion();

        builder.HasIndex(pa => pa.Status);
        builder.HasIndex(pa => pa.ExpiresOn);
        builder.HasIndex(pa => pa.TenantId);
    }
}
