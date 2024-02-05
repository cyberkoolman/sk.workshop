using Microsoft.SemanticKernel;
using DataServices.Plugins;
using Kernel = Microsoft.SemanticKernel.Kernel;
using Microsoft.SemanticKernel.Planning.Handlebars;

var builder = Kernel.CreateBuilder();

// Configure AI backend used by the kernel
var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

if (useAzureOpenAI)
    builder.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
else
    builder.AddOpenAIChatCompletion(model, apiKey, orgId);

// Build the kernel
var kernel = builder.Build();
kernel.ImportPluginFromType<PopulationPlugin>();

var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "MailPlugin");
kernel.ImportPluginFromPromptDirectory(pluginsDirectory, "MailPlugin");

#pragma warning disable SKEXP0060 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var planner = new HandlebarsPlanner();

// var ask = "Write a mail to share the number of the United States population in 2015 for corporate researchers.";
var ask = "Write a mail to share the number of the United States population in 2015 with gender distribution information for corporate researchers.";

HandlebarsPlan plan;

if (!File.Exists("plan.txt"))
{
    // Create the plan
    plan = await planner.CreatePlanAsync(kernel, ask);
    Console.WriteLine(plan);

    var serializedPlan = plan.ToString();
    await File.WriteAllTextAsync("plan.txt", serializedPlan);
}
else
{
    string serializedPlan = await File.ReadAllTextAsync("plan.txt");
    plan = new HandlebarsPlan(serializedPlan);
}

// Execute the plan
var originalPlanResult = await plan.InvokeAsync(kernel);
Console.WriteLine(originalPlanResult);
