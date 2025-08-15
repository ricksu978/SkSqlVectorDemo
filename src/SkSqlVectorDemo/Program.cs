using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using System.Text;

// 1) Config
var conn = "Server=127.0.0.1,1433;Database=sqldb-embeddinglab;User ID=sa;Password=yourStrong(!)Password;Encrypt=True;TrustServerCertificate=True;";
var azureEndpoint = "https://openai-common-global.openai.azure.com";
var embedDeploymentOrModel = "text-embedding-ada-002";
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// 2) Embedding generator (Microsoft.Extensions.AI + OpenAI .NET)
var embeddingClient = new AzureOpenAIClient(
    new Uri(azureEndpoint),
    new AzureCliCredential()
).GetEmbeddingClient(embedDeploymentOrModel);
    
var embeddingGenerator = embeddingClient.AsIEmbeddingGenerator();

// 3) Vector store / collection (SQL Server)
var collection = new SqlServerCollection<string, Policy>(conn, "Policies");
await collection.EnsureCollectionDeletedAsync();
await collection.EnsureCollectionExistsAsync(); 

// 4) Seed Policies
var policies = new[] {
    new Policy { Title="Leave policy",  Content="Employees can work from home on Fridays." },
    new Policy { Title="Expense tip",   Content="Use the corporate card for travel bookings." },
    new Policy { Title="Support hours", Content="Helpdesk is available 8am–6pm AEST on weekdays." },
};

foreach (var policy in policies)
{
    var emb = await embeddingGenerator.GenerateAsync(policy.Content);
    policy.ContentEmbedding = emb.Vector;
    await collection.UpsertAsync(policy);
}

// 5) Query
string? query = "Can I stay home on Friday?";
Console.WriteLine("?> " + query);
var qEmb = await embeddingGenerator.GenerateAsync(query);

await foreach (var hit in collection.SearchAsync(qEmb.Vector, top: 3))
    Console.WriteLine($"{hit.Score:F3}  {hit.Record.Title}  →  {hit.Record.Content}");
Console.Write("\n?> ");

while ((query = Console.ReadLine()) is not null)
{
    qEmb = await embeddingGenerator.GenerateAsync(query);
    await foreach (var hit in collection.SearchAsync(qEmb.Vector, top: 3))
        Console.WriteLine($"{hit.Score:F3}  {hit.Record.Title}  →  {hit.Record.Content}");
    Console.Write("\n?> ");
}

Console.WriteLine("\nDone.");
