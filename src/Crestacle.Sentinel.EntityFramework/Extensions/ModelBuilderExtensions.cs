using Crestacle.Sentinel.Core.Entities;
using Crestacle.Sentinel.EntityFramework.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Sentinel.EntityFramework.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies all Sentinel auth table configurations (auth schema).
    /// Call this from your DbContext.OnModelCreating().
    /// The default configuration uses IsRowVersion() which maps to SQL Server's rowversion type.
    /// For PostgreSQL, call UseSentinelPostgreSqlConcurrency() after this method.
    /// </summary>
    public static ModelBuilder ApplySentinelConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new PermissionConfiguration());
        builder.ApplyConfiguration(new UserRoleConfiguration());
        builder.ApplyConfiguration(new RolePermissionConfiguration());
        builder.ApplyConfiguration(new AuditEntryConfiguration());
        builder.ApplyConfiguration(new PermissionConflictConfiguration());
        builder.ApplyConfiguration(new PendingAssignmentConfiguration());
        return builder;
    }

    /// <summary>
    /// Configures Role, Permission, and PendingAssignment to use PostgreSQL's xmin system column
    /// as a concurrency token instead of the SQL Server-specific bytea RowVersion column.
    /// Call this after ApplySentinelConfiguration() when targeting PostgreSQL.
    /// </summary>
    /// <example>
    /// protected override void OnModelCreating(ModelBuilder builder)
    /// {
    ///     builder.ApplySentinelConfiguration()
    ///            .UseSentinelPostgreSqlConcurrency();
    /// }
    /// </example>
    public static ModelBuilder UseSentinelPostgreSqlConcurrency(this ModelBuilder builder)
    {
        // xmin is a PostgreSQL system column (type xid = uint) that increments on every row update.
        // Mapping it as a shadow concurrency token replaces the SQL-Server-specific IsRowVersion().
        ConfigureXmin<Role>(builder);
        ConfigureXmin<Permission>(builder);
        ConfigureXmin<PendingAssignment>(builder);

        // Ignore the CLR RowVersion property on all three entities.
        // Without this, IsRowVersion() from the default configurations still generates a
        // `row_version bytea NOT NULL` column in PostgreSQL migrations — but PostgreSQL has no
        // mechanism to auto-populate it on INSERT, causing a null-constraint violation in the seeder.
        // xmin handles optimistic concurrency natively; the bytea column is redundant and harmful.
        builder.Entity<Permission>().Ignore(p => p.RowVersion);
        builder.Entity<Role>().Ignore(r => r.RowVersion);
        builder.Entity<PendingAssignment>().Ignore(pa => pa.RowVersion);

        return builder;
    }

    private static void ConfigureXmin<TEntity>(ModelBuilder builder) where TEntity : class
    {
        builder.Entity<TEntity>()
            .Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
