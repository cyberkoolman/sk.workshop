using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;

var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
    .Build();

var ask = @"I need to come up with a few samples of my highschool graduation speech.
Write write them in the form of short but elegant poem.";

# pragma warning disable SKEXP0060
var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions{ AllowLoops = true });

var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "plugins");
kernel.ImportPluginFromPromptDirectory(pluginsDirectory, "SummarizePlugin");
kernel.ImportPluginFromPromptDirectory(pluginsDirectory, "WriterPlugin");

bool success = false;
int retryCount = 0;
int retrySeconds = 3;
int maxRetries = 5;

string planPrompt = null;
string planResult = null;
while (!success && retryCount < maxRetries)
{
    try
    {
        var plan = await planner.CreatePlanAsync(kernel, ask);
        var planInvoked = await plan.InvokeAsync(kernel);

        planPrompt = plan.ToString();
        planResult = planInvoked;
        success = true;
    }
    catch (Exception)
    {
        Console.WriteLine($"Error occurred while execting plan, regenerating and executing a plan after {retrySeconds} seconds...");
        retryCount++;
        Thread.Sleep(retrySeconds * 1000);
    }
}

if (success)
{
    Console.WriteLine($"Plan:\n{planPrompt}");
    Console.WriteLine($"Plan results:\n{planResult}");
} else 
{
    Console.WriteLine($"After {maxRetries} executions, still failed to execute plan.  Please try again later.");
}
