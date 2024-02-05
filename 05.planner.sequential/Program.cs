// Here, we use predefined plugins located in plugins direcotry

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using System.Text.Json;

// Step 1: Configure AI backend used by the kernel
var builder = new KernelBuilder();
var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

if (useAzureOpenAI)
    builder.WithAzureOpenAIChatCompletionService(model, azureEndpoint, apiKey);
else
    builder.WithOpenAIChatCompletionService(model, apiKey, orgId);

var kernel = builder.Build();

// Step 2: Provide plugins to the planner
var planner = new SequentialPlanner(kernel);

var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "plugins");
kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "SummarizePlugin");
kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "WriterPlugin");

// Step 3: Define the ASK.  What do you want the Kernel to do?
var ask = @"I need to come up with a few samples of my highschool graduation speech.
Write them in the form of short but elegant poem.";

// Step 4: Create a plan
var plan = await planner.CreatePlanAsync(ask);

// Verify if the Planner has taken the user's ask and converted it into a Plan object 
// detailing how the AI would go about solving this task.
// It uses the plugins that the Kernel can use and determines which functions to call in order to fulfill the user's ask.
Console.WriteLine("Plan:\n");
Console.WriteLine(JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true }));

// Step 5: Excute the plan
var planResult = await kernel.RunAsync(plan);

Console.WriteLine("Plan results:\n");
Console.WriteLine(Utils.WordWrap(planResult.GetValue<string>(), 100));