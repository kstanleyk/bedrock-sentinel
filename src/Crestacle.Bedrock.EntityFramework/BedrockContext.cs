using Crestacle.Bedrock.Core.Defaults;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces;
using Crestacle.Bedrock.EntityFramework.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework;

public class BedrockContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    private string? CurrentTenantId => _tenantContext.GetTenantId();

    public BedrockContext(DbContextOptions options, ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext ?? new NullTenantContext();
    }

    public DbSet<UserCredential> UserCredentials { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<MfaChallenge> MfaChallenges { get; set; } = null!;
    public DbSet<StepUpChallenge> StepUpChallenges { get; set; } = null!;
    public DbSet<OtpCode> OtpCodes { get; set; } = null!;
    public DbSet<RecoveryCode> RecoveryCodes { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<KnownDevice> KnownDevices { get; set; } = null!;
    public DbSet<PasswordHistory> PasswordHistories { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;
    public DbSet<ConsentRecord> ConsentRecords { get; set; } = null!;
    public DbSet<EmailChangeToken> EmailChangeTokens { get; set; } = null!;
    public DbSet<MagicLinkToken> MagicLinkTokens { get; set; } = null!;
    public DbSet<PasskeyCredential> PasskeyCredentials { get; set; } = null!;
    public DbSet<ExternalIdentity> ExternalIdentities { get; set; } = null!;
    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BedrockEntityFrameworkAssemblyMarker).Assembly);

        ApplyUtcConverters(modelBuilder);
        ApplyTenantFilters(modelBuilder);
    }

    private static void ApplyUtcConverters(ModelBuilder modelBuilder)
    {
        var utcConverter = new UtcDateTimeConverter();
        var nullableUtcConverter = new NullableUtcDateTimeConverter();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserCredential>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<RefreshToken>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<MfaChallenge>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<StepUpChallenge>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<OtpCode>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<RecoveryCode>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<Session>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<KnownDevice>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<PasswordHistory>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<EmailVerificationToken>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<PasswordResetToken>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<AuditEntry>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<ConsentRecord>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<EmailChangeToken>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<MagicLinkToken>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<PasskeyCredential>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<ExternalIdentity>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<Invitation>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);

        modelBuilder.Entity<ApiKey>().HasQueryFilter(
            e => CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == null);
    }
}
