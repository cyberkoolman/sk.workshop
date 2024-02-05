using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using LLama.Common;
using LLama;

// https://huggingface.co/TheBloke/Llama-2-7B-Chat-GGUF/tree/main
string modelPath = "C:\\Projects\\OpenAI.Workshop\\models\\llama-2-7b-chat.Q4_0.gguf"; // change it to your own model path

// Load weights into memory
var parameters = new ModelParams(modelPath);
using var model = LLamaWeights.LoadFromFile(parameters);
var ex = new StatelessExecutor(model, parameters);

var builder = Kernel.CreateBuilder();
builder.Services.AddKeyedSingleton<ITextGenerationService>("local-llama", new LLamaSharpTextCompletion(ex));

var kernel = builder.Build();

var prompt = @"{{$input}}

One line TLDR with the fewest words.";

ChatRequestSettings settings = new() { MaxTokens = 100 };
var summarize = kernel.CreateFunctionFromPrompt(prompt, settings);

string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

Console.WriteLine((await kernel.InvokeAsync(summarize, new() { ["input"] = text1 })).GetValue<string>());
