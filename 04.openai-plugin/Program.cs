using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;

// Get Settings from Settings.json
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
    .Build();

var input = "I like to buy a laptop with the budge of $500, show me at least 3 products";
var prompt = """
Statement: {{$input}}        
From above statement, create a JSON with only extract product_type, budget_amount as number and quantity.
""";

var jsonPrompt = kernel.CreateFunctionFromPrompt(prompt);
var jsonResult = await kernel.InvokeAsync(jsonPrompt, new() { ["input"] = input });
var data = JsonSerializer.Deserialize<JsonElement>(jsonResult.GetValue<string>());  

#pragma warning disable SKEXP0042
var plugin = await kernel.ImportPluginFromOpenAIAsync("Klarna", new Uri("https://www.klarna.com/.well-known/ai-plugin.json"));

var arguments = new KernelArguments();
arguments["q"] = data.GetProperty("product_type");          // Category or product that needs to be searched for.
arguments["size"] = data.GetProperty("quantity").ToString();           // Number of products to return
arguments["max_price"] = data.GetProperty("budget_amount").ToString();    // Maximum price of the matching product in local currency
arguments["countryCode"] = "US";    // ISO 3166 country code with 2 characters based on the user location.
                                    // Currently, only US, GB, DE, SE and DK are supported.

var functionResult = await kernel.InvokeAsync(plugin["productsUsingGET"], arguments);

var result = functionResult.GetValue<RestApiOperationResponse>();
  
var jsonObject = JsonDocument.Parse(result.Content.ToString());  
var prettyJson = JsonSerializer.Serialize(jsonObject.RootElement, new JsonSerializerOptions { WriteIndented = true });  
Console.WriteLine(prettyJson);
#pragma warning restore SKEXP0042
