using LLama.Common;
using LLama;

// https://huggingface.co/TheBloke/Llama-2-7B-Chat-GGUF/tree/main
string modelPath = "C:\\Projects\\OpenAI.Workshop\\models\\llama-2-7b-chat.Q4_0.gguf"; // change it to your own model path

// Load model
var parameters = new ModelParams(modelPath)
{
    ContextSize = 1024
};
using var model = LLamaWeights.LoadFromFile(parameters);

// Initialize a chat session
using var context = model.CreateContext(parameters);
var ex = new InteractiveExecutor(context);
ChatSession session = new ChatSession(ex);

// show the prompt
var prompt = "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.\r\n\r\n User: Hello, Bob.\r\nAssistant: Hello. How may I help you today?";
Console.WriteLine();
Console.WriteLine(prompt);

ChatHistory history = new ChatHistory();
history.AddMessage(AuthorRole.User, "Hello, Bob.");
history.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?");
Console.Write(" User: ");
while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    prompt = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;
    history.AddMessage(AuthorRole.User, prompt);

    var reply = "";
    await foreach (var text in session.ChatAsync(history, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:" } }))
    {
        Console.Write(text);
        reply += text;
    }
    history.AddMessage(AuthorRole.Assistant, reply);
}