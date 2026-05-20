using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.DataModel;

/// <summary>
/// Asserts that <see cref="AuditEntry"/> has no foreign-key relationship to
/// <see cref="UserCredential"/>. The absence of an FK is the intentional design
/// choice that prevents cascade-delete from silently removing audit history.
/// This test locks in the intent so that a future change adding an FK is caught immediately.
/// </summary>
public sealed class AuditEntryNoCascadeTest : IDisposable
{
    private readonly TestBedrockContext _dbContext;
    private readonly SqliteConnection _connection;

    public AuditEntryNoCascadeTest()
        => (_dbContext, _connection) = DbContextFactory.Create();

    [Fact]
    public void AuditEntry_HasNoForeignKeysInEfModel()
    {
        var auditEntityType = _dbContext.Model.FindEntityType(typeof(AuditEntry));

        auditEntityType.Should().NotBeNull();
        auditEntityType!.GetForeignKeys().Should().BeEmpty(
            because: "AuditEntry rows must survive UserCredential deletion — an FK " +
                     "with any cascade behaviour would either delete audit history silently " +
                     "or block credential removal");
    }

    [Fact]
    public void AuditEntry_UserId_IsNotAForeignKeyColumn()
    {
        var auditEntityType = _dbContext.Model.FindEntityType(typeof(AuditEntry))!;
        var userCredEntityType = _dbContext.Model.FindEntityType(typeof(UserCredential))!;

        var fksPointingToUserCred = auditEntityType
            .GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType == userCredEntityType);

        fksPointingToUserCred.Should().BeEmpty(
            because: "UserId on AuditEntry is a plain column, not a FK, by design");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
