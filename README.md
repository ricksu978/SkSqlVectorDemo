# AI Hack Day — SQL Server + Semantic Kernel Vector Store (15‑min demo)

This is a **tiny, newbie‑friendly demo** showing vector search using the **Semantic Kernel SQL Server Vector Store connector (Preview)** and **Microsoft.Extensions.AI** for embeddings.

## Prereqs
- .NET 8 SDK
- **Azure SQL** or **SQL Server 2025 (17.x) Preview** (for the `VECTOR` type)
- An embedding provider:
  - **OpenAI** (default in this sample), or
  - **Azure OpenAI** (swap a few lines—see below)

> SQL Server vector data type docs: https://learn.microsoft.com/sql/t-sql/data-types/vector-data-type

## 1) Get packages (pre‑release)
```bash
dotnet new console -n throwaway -f net8.0 && rm -rf throwaway   # ensure SDK works

# From inside src/SkSqlVectorDemo
dotnet add package Microsoft.SemanticKernel.Connectors.SqlServer --prerelease
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
```

## 2) Configure environment
```bash
# SQL connection (use Azure SQL or local SQL 2025 preview)
setx SQLSERVER_CONN "Server=localhost;Database=SkDemo;Integrated Security=true;TrustServerCertificate=true;"

# OpenAI (or set AZURE_OPENAI_API_KEY + use Azure client below)
setx OPENAI_API_KEY "<your_openai_key>"
setx OPENAI_EMBED_MODEL "text-embedding-3-small"
```

> On macOS/Linux use `export` instead of `setx`. For Azure SQL, use a proper ADO.NET connection string.

## 3) Run
```bash
cd src/SkSqlVectorDemo
dotnet run
```

You should see the **"Leave policy"** note ranked highest for the query “Can I stay home on Friday?”.

## Using Azure OpenAI instead (optional)
Replace the embedding client setup in `Program.cs` with:
```csharp
using OpenAI;
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
var aoaiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBED_DEPLOYMENT") ?? "text-embedding-3-small";

var aoai = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(aoaiKey));
var embeddingClient = aoai.GetEmbeddingClient(deployment);
IEmbeddingGenerator<string, Embedding<float>> embedGen = embeddingClient.AsIEmbeddingGenerator();
```

## Notes / Limitations
- The **SQL connector uses a Flat vector index** today; great for workshops and small datasets.
- ANN in SQL (e.g., `CREATE VECTOR INDEX ... TYPE='diskann'` + `VECTOR_SEARCH`) is available in SQL Server 2025 preview, but **not** used by this connector yet.
- APIs are **Preview** and may change.

## Credits / Links
- SK SQL Server connector (Preview): https://learn.microsoft.com/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/sql-connector
- Vector Store concepts: https://learn.microsoft.com/semantic-kernel/concepts/vector-store-connectors/
- IEmbeddingGenerator: https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai.openaiclientextensions.asiembeddinggenerator
- SQL VECTOR type: https://learn.microsoft.com/sql/t-sql/data-types/vector-data-type
- DiskANN improvements: https://devblogs.microsoft.com/azure-sql/sql-server-2025-ctp-2-1-diskann-improvements/
