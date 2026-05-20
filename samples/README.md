# Samples

Three runnable sample applications, one per deployment model. Each uses SQLite so no external database is required.

| Sample | Model | What it shows |
|---|---|---|
| [Embedded](Embedded/) | Embedded | Auth and business data in one app and database |
| [Standalone](Standalone/) | Standalone auth service | Dedicated auth microservice, no business data |
| [ExternalIdp](ExternalIdp/) | External IDP | Bedrock with an external token issuer |

See [deployment models](../docs/deployment-models.md) for a full explanation of each pattern.

## Running a sample

```bash
cd samples/Embedded        # or Standalone, ExternalIdp
dotnet run
```

The database is created automatically on first run. All samples log emails to the console instead of sending them — registration confirmation tokens and password reset links appear in the terminal output.

## Using NuGet packages instead of project references

The sample `.csproj` files reference the library source directly via `<ProjectReference>`. To use the published NuGet packages instead, replace the `<ProjectReference>` items with:

```xml
<PackageReference Include="Crestacle.Bedrock.AspNetCore" Version="1.4.2" />
<PackageReference Include="Crestacle.Bedrock.EntityFramework" Version="1.4.2" />
```
