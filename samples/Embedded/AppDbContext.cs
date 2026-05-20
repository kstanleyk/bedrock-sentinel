using Crestacle.Bedrock.EntityFramework;
using Embedded.Models;
using Microsoft.EntityFrameworkCore;

namespace Embedded;

public class AppDbContext(DbContextOptions<AppDbContext> options) : BedrockContext(options)
{
    // Bedrock auth tables (Users, Sessions, ApiKeys, etc.) come from BedrockContext.
    // Add your application's own tables here.
    public DbSet<Order> Orders { get; set; } = null!;
}
