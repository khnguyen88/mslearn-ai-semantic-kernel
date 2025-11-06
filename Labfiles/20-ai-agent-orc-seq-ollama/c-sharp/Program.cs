using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using OllamaSharp;
using System.ComponentModel;
using System.Net.Http.Json;

Console.Clear();
// Install packages for Agent Orchestration
// https://github.com/microsoft/semantic-kernel/issues/12453
// dotnet add package Microsoft.SemanticKernel.Agents.Runtime.InProcess --prerelease
// dotnet add package Microsoft.SemanticKernel.Agents.Orchestration --prerelease
// dotnet add package Microsoft.SemanticKernel.Agents.Magentic --prerelease

// Obtain your API access information
// ---------------------------------------------------------------
//string filePath = Path.GetFullPath("appsettings.json");
//var config = new ConfigurationBuilder()
//    .AddJsonFile(filePath)
//    .Build();

var config = new ConfigurationBuilder().AddUserSecrets("d8159b05-7d55-4374-8785-8a023929f4ce").Build();


// Set your values in appsettings.json (not used in this example)
// ---------------------------------------------------------------
//string apiKey = config["PROJECT_KEY"]!;
//string endpoint = config["PROJECT_ENDPOINT"]!;
//string deploymentName = config["DEPLOYMENT_NAME"]!;

// Set your values in secret.json;
// ---------------------------------------------------------------
string deploymentName = config["AzureAIFoundry:AIModel:Name"]!;
string endpoint = config["AzureAIFoundry:AIModel:Uri"]!;
string apiKey = config["AzureAIFoundry:AIModel:ApiKey"]!;


// Create a kernel builder with Azure OpenAI chat completion
// ---------------------------------------------------------------
#pragma warning disable
var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddOllamaTextGeneration(
    modelId: "tinyllama",
    endpoint: new Uri("http://localhost:11434")
);

kernelBuilder.AddOllamaChatCompletion(
    modelId: "tinyllama",
    endpoint: new Uri("http://localhost:11434")
);

var kernel = kernelBuilder.Build();



// Create Chat Completion Agents (Extra)
// =====================================================================================
ChatCompletionAgent writerAgent =
    new()
    {
        Name = "WriterAgent",
        Instructions = "You are a short story author and writer. You like to write a funny story in 50 words or less. Write in slang.",
        Description = "An agent that writes a story in 50 words or less.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


ChatCompletionAgent editorAgent =
    new()
    {
        Name = "EditorAgent",
        Instructions = "You are an editor agent. You will fix any grammtical errors or spelling errors in a story sent your way. You can make the tone of the story more positive",
        Description = "An agent that assists an author or writer in their edits.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent publisherAgent =
    new()
    {
        Name = "PublisherAgent",
        Instructions = "You are a publisher agent. You will come up with the title and short one sentence description for the story sent to you. You will provide an ISBN number.",
        Description = "An agent who provide the title and description for the story shared to them",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };



// Manages chat history and develop callback to caputre agent responses
// =====================================================================================
ChatHistory history = [];

ValueTask responseCallback(ChatMessageContent response)
{
    history.Add(response);
    return ValueTask.CompletedTask;
}


// Create a sequential orchestration
// ---------------------------------------------------------------

SequentialOrchestration orchestration = new(writerAgent, editorAgent, publisherAgent)
{

    ResponseCallback = responseCallback,
};

//THIS WORKS
using var ollamaClient = new OllamaApiClient(
            uriString: "http://localhost:11434",
            defaultModel: "tinyllama");

var chatService = ollamaClient.AsChatCompletionService();

var chatHistory = new ChatHistory("You are a helpful assistant that knows about AI.");

chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");



// THIS WORKS TOO
var reply = await chatService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine(reply.Content);


var client = new HttpClient();
var payload = new
{
    model = "tinyllama",
    prompt = "Tell me some plant information about pumpkin, like their ideal soil moisture and light intensity in terms of % needs",
    stream = false,
    options = new
    {
        max_tokens = 50,
        temperature = 0.7
    }

};

var response = await client.PostAsJsonAsync("http://localhost:11434/api/generate", payload);
var result2 = await response.Content.ReadAsStringAsync();
Console.WriteLine(result2);



var agentThread = new ChatHistoryAgentThread();


var response4 = writerAgent.InvokeAsync("tell me a short story in 10 words or less.", agentThread);
await foreach (var item in response4)
{
    Console.WriteLine(item.Message);
}


// Start the runtime
// =====================================================================================
// A runtime is required to manage the execution of agents. Here, we use InProcessRuntime and start it before invoking the orchestration.
InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


// Get user input
// Invoke the orchestration
// ====================================================================================
Console.WriteLine("What should we write a story about?");
string input = string.Empty;
input = Console.ReadLine();
// Invoke the orchestration
// ====================================================================================

Console.WriteLine($"\n# INPUT: {input}\n");
OrchestrationResult<string> result = await orchestration.InvokeAsync(input, runtime);


// Console Conversation
// =====================================================================================
string text = await result.GetValueAsync(TimeSpan.FromSeconds(30));
Console.WriteLine($"\n# CHAT ORCHESTRATION RESULT: {text}");
Console.WriteLine("\n\nORCHESTRATION HISTORY: ");
foreach (ChatMessageContent message in history)
{
    Console.WriteLine($"{message.AuthorName}:");
    Console.WriteLine($"{message.Content}");
    Console.WriteLine("\n");


}

// Stop the Runtime
// ====================================================================================
// After processing is complete, stop the runtime to clean up resources.
await runtime.RunUntilIdleAsync();
#pragma warning restore