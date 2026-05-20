using Crestacle.Bedrock.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

internal static class DbContextFactory
{
    public static (TestBedrockContext context, SqliteConnection connection) Create(
        ITenantContext? tenantContext = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestBedrockContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestBedrockContext(options, tenantContext);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
