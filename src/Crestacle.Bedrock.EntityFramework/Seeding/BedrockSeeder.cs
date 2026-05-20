namespace Crestacle.Bedrock.EntityFramework.Seeding;

/// <summary>
/// Base class for seeding initial data into a Bedrock-enabled database.
/// Override <see cref="SeedAsync"/> to add application-specific seed data.
/// </summary>
public abstract class BedrockSeeder
{
    public virtual Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
        => Task.CompletedTask;
}
