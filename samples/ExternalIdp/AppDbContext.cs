using Crestacle.Bedrock.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ExternalIdp;

public class AppDbContext(DbContextOptions<AppDbContext> options) : BedrockContext(options)
{
}
