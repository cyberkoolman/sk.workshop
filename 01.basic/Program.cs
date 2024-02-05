using Microsoft.SemanticKernel;

// Get Settings from Settings.json
var (useAzureOpenAI, chatDeployment, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

var kernel = useAzureOpenAI ? Kernel.CreateBuilder()
                                .AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, apiKey)
                                .Build()
                            : Kernel.CreateBuilder()
                                .AddOpenAIChatCompletion("gpt-3.5-turbo", apiKey, orgId)
                                .Build();


// Case 1. Just Ask a question, and the question is the prompt
var question = "What is Contoso Electronics?";
var answer = await kernel.InvokePromptAsync(question);
Console.WriteLine(answer.GetValue<string>());


/*
// Case 2: Define and Configure Inline Prompt and a sample input text
var input = """
Demo (ancient Greek poet)
From Wikipedia, the free encyclopedia
Demo or Damo (Greek: Δεμώ, Δαμώ; fl.c.AD 200) was a Greek woman of the Roman period, known for a single epigram, engraved upon the Colossus of Memnon, which bears her name. She speaks of herself therein as a lyric poetess dedicated to the Muses, but nothing is known of her life.[1]
Identity
Demo was evidently Greek, as her name, a traditional epithet of Demeter, signifies. The name was relatively common in the Hellenistic world, in Egypt and elsewhere, and she cannot be further identified. The date of her visit to the Colossus of Memnon cannot be established with certainty, but internal evidence on the left leg suggests her poem was inscribed there at some point in or after AD 196.[2]
Epigram
There are a number of graffiti inscriptions on the Colossus of Memnon. Following three epigrams by Julia Balbilla, a fourth epigram, in elegiac couplets, entitled and presumably authored by "Demo" or "Damo" (the Greek inscription is difficult to read), is a dedication to the Muses.[2] The poem is traditionally published with the works of Balbilla, though the internal evidence suggests a different author.[1]
In the poem, Demo explains that Memnon has shown her special respect. In return, Demo offers the gift for poetry, as a gift to the hero. At the end of this epigram, she addresses Memnon, highlighting his divine status by recalling his strength and holiness.[2]
Demo, like Julia Balbilla, writes in the artificial and poetic Aeolic dialect. The language indicates she was knowledgeable in Homeric poetry—'bearing a pleasant gift', for example, alludes to the use of that phrase throughout the Iliad and Odyssey.[a][2] 
""";

string promptString = """
{{$content}}

Summarize the content above to TLDR with 4 bullet points.
""";

var summaryFunction = kernel.CreateFunctionFromPrompt(promptString);
var summaryResult = await kernel.InvokeAsync(summaryFunction, new() { ["content"] = input });
Console.WriteLine(summaryResult);
*/

/*
const string chatPrompt = @"
{{$history}}
User: {{$input}}
ChatBot:";

var chatFunction = kernel.CreateFunctionFromPrompt(chatPrompt);
var history = "";
var arguments = new KernelArguments()
{
    ["history"] = history
};

Func<string, Task> Chat = async (string input) => {
    arguments["input"] = input;
    var answer = await chatFunction.InvokeAsync(kernel, arguments);

    // Append the new interaction to the chat history
    var result = $"\nUser: {input}\nChatBot: {answer}\n";
    history += result;
    arguments["history"] = history;
    
    Console.WriteLine(result);
};

await Chat("Hi, I'm looking for book suggestions.");
await Chat("Shoulb be about Greece history. Please only give me one suggestion.");
await Chat("that sounds interesting, what are some of the topics I will learn about?");
*/