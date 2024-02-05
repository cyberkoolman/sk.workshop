using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

// Configure AI service credentials used by the kernel
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
    .Build();
var question = "what is 1234.56789*56789.1234?";

// Console.WriteLine(question);
// var result = await kernel.InvokePromptAsync(question);
// Console.WriteLine(result);

/*
#pragma warning disable SKEXP0050
// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Plugins/Plugins.Core/MathPlugin.cs
kernel.ImportPluginFromType<MathPlugin>();

OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};
Console.WriteLine(question);
var result = await kernel.InvokePromptAsync(question, new (settings));
Console.WriteLine(result);
#pragma warning restore SKEXP0050
*/

kernel.ImportPluginFromType<CustomPlugins.MathPlugin>();
OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};
Console.WriteLine(question);
var result = await kernel.InvokePromptAsync(question, new (settings));
Console.WriteLine(result);


/*
// If manual function execution
OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
};

. . . .

var functionCalls = someResult.GetOpenAIFunctionToolCalls();
foreach (var functionCall in functionCalls)
{
    KernelFunction pluginFunction;
    KernelArguments arguments;
    kernel.Plugins.TryGetFunctionAndArguments(functionCall, out pluginFunction, out arguments);
    var functionResult = await kernel.InvokeAsync(pluginFunction!, arguments!);
    var jsonResponse = functionResult.GetValue<object>();
    var json = JsonSerializer.Serialize(jsonResponse);
    Console.WriteLine(json);
}

*/