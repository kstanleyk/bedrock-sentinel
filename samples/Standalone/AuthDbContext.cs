using Crestacle.Bedrock.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Standalone;

// No business tables — auth schema only.
// This is the entire database for a dedicated auth service.
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : BedrockContext(options)
{
}
