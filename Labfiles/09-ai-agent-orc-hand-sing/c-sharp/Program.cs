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
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;


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
ChatCompletionAgent receiptionistHelper =
    new()
    {
        Name = "Receiptionist",
        Instructions = "You are a receiptionist and generalist. You will try to handoff the questions to an expert that can best answer it. However, if there are no experts, then you must inform the users that there are no expert in our handoff group that can answer their question or request. Please apologize to them and request they answer another question.",
        Description = "A receiptionist in the handoff orchestration group.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent chemistExpert =
    new()
    {
        Name = "ChemistExpert",
        Instructions = "You are an expert in chemistry. You answer questions from a chemist's perspective.",
        Description = "A chemistry expert in the handoff orchestration group.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


ChatCompletionAgent historianExpert =
    new()
    {
        Name = "HistorianExpert",
        Instructions = "You are an expert in history. You answer questions from a historian's perspective.",
        Description = "A history expert in the handoff orchestration group.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent engineeringExpert =
    new()
    {
        Name = "EngineeringExpert",
        Instructions = "You are an expert in engineering. You answer questions from a engineer's perspective.",
        Description = "An engineering expert in the handoff orchestration group.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

// Set Up Handoff Relationships
// =====================================================================================
var handoffs = OrchestrationHandoffs
    .StartWith(receiptionistHelper)
    .Add(receiptionistHelper, chemistExpert, historianExpert, engineeringExpert)
    .Add(chemistExpert, receiptionistHelper, "Transfer to this agent if the issue is not chemistry related.")
    .Add(historianExpert, receiptionistHelper, "Transfer to this agent if the issue is not history related.")
    .Add(engineeringExpert, receiptionistHelper, "Transfer to this agent if the issue is not engineering related.");


// Manages chat history and develop callback to caputre agent responses
// =====================================================================================
// You can create a callback to capture agent responses as the conversation progresses via the ResponseCallback property.

ChatHistory history = [];

ValueTask responseCallback(ChatMessageContent response)
{
    history.Add(response);
    return ValueTask.CompletedTask;
}


// Include human participants in handoff orchestration conversation with the InteractiveCallback
// =====================================================================================
Console.WriteLine("What topics do you want a group of expert perspective on?");
string question = string.Empty;
question = Console.ReadLine();

// ValueTask<ChatMessageContent> interactiveCallback()
// {
//     Console.Write("User Question: ");
//     string? input = Console.ReadLine();
//     Console.WriteLine("------------------------");

//     return ValueTask.FromResult(new ChatMessageContent(AuthorRole.User, input));

// }


// Create a handoff orchestration
// =====================================================================================
HandoffOrchestration orchestration = new HandoffOrchestration(handoffs, receiptionistHelper, chemistExpert, historianExpert, engineeringExpert)
{
    //InteractiveCallback = interactiveCallback,
    ResponseCallback = responseCallback,
};


// Start the runtime
// =====================================================================================
// A runtime is required to manage the execution of agents. Here, we use InProcessRuntime and start it before invoking the orchestration.
InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


// Invoke the orchestration
// ====================================================================================
//Console.WriteLine($"\n# INPUT: {input}\n");
OrchestrationResult<string> result = await orchestration.InvokeAsync(question, runtime);


// Console Conversation
// =====================================================================================
string ouput = await result.GetValueAsync(TimeSpan.FromSeconds(30));
Console.WriteLine($"\n# CONCURRENT ORCHESTRATION RESULT: {ouput}");
Console.WriteLine("\n\nORCHESTRATION HISTORY: ");
foreach (ChatMessageContent message in history)
{
    Console.WriteLine($"{message.Role} - {message.AuthorName}: ");
    Console.WriteLine($"{message.Content}");
    Console.WriteLine("\n");
}


// Stop the Runtime
// ====================================================================================
// After processing is complete, stop the runtime to clean up resources.
await runtime.RunUntilIdleAsync();

Console.WriteLine("\nChat session ended.");

#pragma warning restore
