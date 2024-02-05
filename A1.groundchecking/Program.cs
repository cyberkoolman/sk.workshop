using Microsoft.SemanticKernel;

// Get Settings from Settings.json
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
    .Build();

var summarizePluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "plugins", "SummarizePlugin");
var summarizePlugin = kernel.ImportPluginFromPromptDirectory(summarizePluginsDirectory);
var summaryText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "ground", "documents", "summary.txt"));

var groundingPluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "plugins", "GroundingPlugin");
var groundingPlugin = kernel.ImportPluginFromPromptDirectory(groundingPluginsDirectory);
var groundingText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "ground", "documents", "grounding.txt"));

var create_summary = summarizePlugin["Summarize"];

KernelArguments variables = new()
{
    ["input"] = summaryText,
    ["topic"] = "people and places",
    ["example_entities"] = "John, Jane, mother, brother, Paris, Rome"
};

Console.WriteLine("======== Extract Entities ========");
var entityExtraction = groundingPlugin["ExtractEntities"];
var extractionResult = (await kernel.InvokeAsync(entityExtraction, variables)).ToString();
Console.WriteLine(extractionResult);

Console.WriteLine("\n======== Reference Check ========");
variables["input"] = extractionResult;
variables["reference_context"] = groundingText;
var reference_check = groundingPlugin["ReferenceCheckEntities"];
var groundingResult = (await kernel.InvokeAsync(reference_check, variables)).ToString();
Console.WriteLine(groundingResult);

Console.WriteLine("\n======== Excise Entities ========");
variables["input"] = summaryText;
variables["ungrounded_entities"] = groundingResult;
var entity_excision = groundingPlugin["ExciseEntities"];
var excisionResult = await kernel.InvokeAsync(entity_excision, variables);
Console.WriteLine(excisionResult.GetValue<string>());
