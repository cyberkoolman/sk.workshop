using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Get Settings from Settings.json
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
    .Build();

#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0022, SKEXP0052
var memoryBuilder = new MemoryBuilder();
if (useAzureOpenAI)
{
    memoryBuilder.WithAzureOpenAITextEmbeddingGeneration("embedding", azureEndpoint, apiKey, "model-id");
}
else
{
    memoryBuilder.WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey);
}
// Option 1: Use a local memory store
// memoryBuilder.WithMemoryStore(new VolatileMemoryStore());

// Option 2: Use a Chroma memory store
/*
git clone https://github.com/chroma-core/chroma.git
cd chroma
docker-compose up --build
*/
var chromaMemoryStore = new ChromaMemoryStore("http://127.0.0.1:8000");
memoryBuilder.WithMemoryStore(chromaMemoryStore);

var memory = memoryBuilder.Build();

const string MemoryCollectionName = "aboutMe";
await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "My name is Andrea");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "I currently work as a tourist operator");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "I currently live in Seattle and have been living there since 2005");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "I visited France and Italy five times since 2015");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info5", text: "My family is from New York");

/*
var question = "Where do I live now?";
var response = await memory.SearchAsync(MemoryCollectionName, question, limit: 1, minRelevanceScore: 0.5).FirstOrDefaultAsync();
Console.WriteLine(question + " " + response.Metadata.Text);
*/

// -- > Using the response, provide it as in-context prompt & implement rest of RAG pattern

// TextMemoryPlugin provides the "recall" function
// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/Plugins/Plugins.Memory/TextMemoryPlugin.cs
// public async Task<string> RecallAsync()
#pragma warning disable SKEXP0052
kernel.ImportPluginFromObject(new TextMemoryPlugin(memory));

const string skPrompt = @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Information about me, from previous conversations:
- {{$fact1}} {{recall $fact1}}
- {{$fact2}} {{recall $fact2}}
- {{$fact3}} {{recall $fact3}}
- {{$fact4}} {{recall $fact4}}
- {{$fact5}} {{recall $fact5}}

Chat:
{{$history}}
User: {{$userInput}}
ChatBot: ";

var chatFunction = kernel.CreateFunctionFromPrompt(
                        skPrompt, 
                        new OpenAIPromptExecutionSettings { 
                            MaxTokens = 200, 
                            Temperature = 0.8 
                        }
                    );

#pragma warning disable SKEXP0052
var arguments = new KernelArguments();

arguments["fact1"] = "what is my name?";
arguments["fact2"] = "where do I live?";
arguments["fact3"] = "where is my family from?";
arguments["fact4"] = "where have I travelled?";
arguments["fact5"] = "what do I do for work?";

arguments[TextMemoryPlugin.CollectionParam] = MemoryCollectionName;
arguments[TextMemoryPlugin.LimitParam] = "1";
arguments[TextMemoryPlugin.RelevanceParam] = "0.5";

var history = "";
arguments["history"] = history;
Func<string, Task> Chat = async (string input) => {
    // Save new message in the kernel arguments
    arguments["userInput"] = input;

    // Process the user message and get an answer
    var answer = await chatFunction.InvokeAsync(kernel, arguments);

    // Append the new interaction to the chat history
    var result = $"\nUser: {input}\nChatBot: {answer}\n";

    history += result;
    arguments["history"] = history;
    
    // Show the bot response
    Console.WriteLine(result);
};

await Chat("I want to plan a trip and visit my family. Do you know where that is?");
await Chat("Great! What are some landmarks around there?");
await Chat("Are there similar ones around where I live now?");