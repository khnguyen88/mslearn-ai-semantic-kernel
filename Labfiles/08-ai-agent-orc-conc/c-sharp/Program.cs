using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;

#pragma warning disable
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
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();


// Create Chat Completion Agents (Extra)
// =====================================================================================
ChatCompletionAgent chemistExpert =
    new()
    {
        Name = "ChemistExpert",
        Instructions = "You are an expert in chemistry. You answer questions from a chemist's perspective.",
        Description = "A chemistry expert.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


ChatCompletionAgent historianExpert =
    new()
    {
        Name = "HistorianExpert",
        Instructions = "You are an expert in history. You answer questions from a historian's perspective.",
        Description = "A history expert.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent engineeringExpert =
    new()
    {
        Name = "EngineeringExpert",
        Instructions = "You are an expert in engineering. You answer questions from a engineer's perspective.",
        Description = "An engineering expert.",
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


// Create a concurrent orchestration
// ---------------------------------------------------------------
ConcurrentOrchestration orchestration = new(chemistExpert, historianExpert, engineeringExpert)
{

    ResponseCallback = responseCallback,
};


// Start the runtime
// =====================================================================================
// A runtime is required to manage the execution of agents. Here, we use InProcessRuntime and start it before invoking the orchestration.
InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


// Get user input
// Invoke the orchestration
// ====================================================================================
Console.WriteLine("What topics do you want a group of expert perspective on?");
string input = string.Empty;
input = Console.ReadLine();


// Invoke the orchestration
// ====================================================================================

Console.WriteLine($"\n# INPUT: {input}\n");
OrchestrationResult<string[]> result = await orchestration.InvokeAsync(input, runtime);


// Console Conversation
// =====================================================================================
string[] texts = await result.GetValueAsync(TimeSpan.FromSeconds(30));
Console.WriteLine($"\n# CONCURRENT ORCHESTRATION RESULT: {string.Join("\n\n", texts.Select(text => $"{text}"))}");
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

Console.WriteLine("\nChat session ended.");
#pragma warning restore