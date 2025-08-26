using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;

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
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
SequentialOrchestration orchestration = new(writerAgent, editorAgent, publisherAgent)
{

    ResponseCallback = responseCallback,
};
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Start the runtime
// =====================================================================================
// A runtime is required to manage the execution of agents. Here, we use InProcessRuntime and start it before invoking the orchestration.
InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


// Get user input
// Invoke the orchestration
// ====================================================================================
Console.WriteLine("What should we right a story about?");
string input = string.Empty;
input = Console.ReadLine();
// Invoke the orchestration
// ====================================================================================
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Console.WriteLine($"\n# INPUT: {input}\n");
OrchestrationResult<string> result = await orchestration.InvokeAsync(input, runtime);


// Console Conversation
// =====================================================================================
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
string text = await result.GetValueAsync(TimeSpan.FromSeconds(30));
Console.WriteLine($"\n# CHAT ORCHESTRATION RESULT: {text}");
Console.WriteLine("\n\nORCHESTRATION HISTORY: ");
foreach (ChatMessageContent message in history)
{
#pragma warning disable SKEXP000
    Console.WriteLine($"{message.AuthorName}:");
    Console.WriteLine($"{message.Content}");
    Console.WriteLine("\n");
#pragma warning disable SKEXP000

}

// Stop the Runtime
// ====================================================================================
// After processing is complete, stop the runtime to clean up resources.
await runtime.RunUntilIdleAsync();

Console.WriteLine("\nChat session ended.");
