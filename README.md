SQL Server Vector Search demo (.NET 8 + Microsoft.Extensions.AI + Azure OpenAI)

This sample seeds a few policies, stores their embeddings in SQL Server via the Semantic Kernel SQL Server connector, and lets you run semantic searches from the console.

What changed in this version
- Uses AzureOpenAIClient + AzureCliCredential (no API key in code). Requires `az login`.
- Uses Microsoft.Extensions.AI `AsIEmbeddingGenerator()` to generate embeddings.
- Interactive console loop for repeated queries.
- Collection is dropped and recreated on each run (`EnsureCollectionDeletedAsync` + `EnsureCollectionExistsAsync`).
- Embedding dimension for `Policy.ContentEmbedding` is 1536 to match `text-embedding-ada-002`.
- Targets .NET 8 and C# 12. NuGet packages updated in the project file.

Prerequisites
- .NET 8 SDK
- SQL Server reachable at your connection string (local install or a container)
- Azure CLI installed and logged in: `az login`
- Azure OpenAI resource with an embeddings deployment (e.g., `text-embedding-ada-002`) and your identity granted access

Configure
- Open `SkSqlVectorDemo/Program.cs` and review:
  - `conn` (SQL Server connection string). Make sure Server, Database, and Password match your environment.
  - `azureEndpoint` (your Azure OpenAI resource endpoint, e.g., `https://<your-resource>.openai.azure.com`).
  - `embedDeploymentOrModel` (your embedding deployment name; default in code is `text-embedding-ada-002`).
- Sign in to Azure CLI: `az login` (the signed-in identity must have access to the Azure OpenAI resource).

Set up SQL Server
- Run a container (optional):
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
- Create the database once (matches default connection string):
  sqlcmd -S 127.0.0.1,1433 -U sa -P "yourStrong(!)Password" -Q "CREATE DATABASE [sqldb-embeddinglab]"

Build and run
- From the repository root:
  dotnet restore
  dotnet run --project SkSqlVectorDemo

What the app does
- Creates a `SqlServerCollection<string, Policy>` named "Policies".
- Deletes and recreates the collection on startup.
- Seeds three Policy records, generates embeddings with the configured Azure OpenAI deployment, and upserts them.
- Runs a top-3 vector search for your query and prints score, title, and content. The console remains interactive for additional queries.

Sample
?> Can I stay home on Friday?
0.873  Leave policy  →  Employees can work from home on Fridays.
...
?>

Changing models
- If you change the embedding model/deployment, ensure `Policy.ContentEmbedding` dimensions match:
  - text-embedding-ada-002 → 1536
  - text-embedding-3-small → 1536
  - text-embedding-3-large → 3072
- Update the `[VectorStoreVector(Dimensions: ...)]` attribute in `SkSqlVectorDemo/Policy.cs` accordingly.

Troubleshooting
- Authentication: run `az login`; ensure your identity has access (e.g., Cognitive Services User) to the Azure OpenAI resource.
- SQL connectivity: verify host/port, DB exists, credentials correct, and `TrustServerCertificate=True` for local dev.
- Empty results: confirm embeddings are generated without exceptions and seeding ran (collection is recreated each run).

Security notes
- Do not hardcode secrets for production. Prefer environment variables, user-secrets, or Azure Key Vault.
- Remove `EnsureCollectionDeletedAsync` in production to avoid dropping data on startup.
