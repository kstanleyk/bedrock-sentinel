using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure.Postgres;

/// <summary>
/// Marks a test as a PostgreSQL integration test. The test is skipped automatically
/// when <c>BEDROCK_TEST_PG_CONNECTION</c> is not set, so the default test run
/// (SQLite, no environment variable) never fails.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(PostgresDbContextFactory.EnvVar)))
            Skip = $"Set {PostgresDbContextFactory.EnvVar} to run PostgreSQL integration tests.";
    }
}
