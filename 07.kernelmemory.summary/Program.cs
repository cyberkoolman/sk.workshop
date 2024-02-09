using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;


// Common Setting: Get Settings from Settings.json
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

// Section 2: Create a Semantic Kernel with local memory
var embbeddingDeplpoyment = Settings.EmbeddingModelKey();

var azureOpenAIEmbeddingConfig = new AzureOpenAIConfig
{
    APIKey = apiKey,
    Deployment = embbeddingDeplpoyment,
    Endpoint = azureEndpoint,
    APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey
};

var azureOpenAITextConfig = new AzureOpenAIConfig
{
    APIKey = apiKey,
    Deployment = chatDeployment,
    Endpoint = azureEndpoint,
    APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey
};

// Kernel Memory
// https://github.com/microsoft/kernel-memory

var kernelMemory = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(azureOpenAITextConfig)
    .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIEmbeddingConfig)
    .Build<MemoryServerless>();

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
var dataFiles = Directory.EnumerateFiles(dataDirectory, "*.*", SearchOption.AllDirectories);
await kernelMemory.ImportDocumentAsync(new Document("summaryDocument")
            .AddFiles(dataFiles),
            steps: Constants.PipelineOnlySummary);

var results = await kernelMemory.SearchSummariesAsync(filter: MemoryFilters.ByDocument("summaryDocument"));

foreach (var result in results)
{
    Console.WriteLine($"== {result.SourceName} summary ==\n{result.Partitions.First().Text}\n");
}
