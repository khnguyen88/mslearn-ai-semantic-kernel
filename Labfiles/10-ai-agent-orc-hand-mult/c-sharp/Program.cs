using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
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
        Instructions = "You are a receiptionist and generalist. You will try to handoff the questions to an expert that can best answer it. However, if there are no experts, then you must inform the users that there are no expert in our handoff group that can answer their question or request. Please apologize to them and request they answer another question. Before handhoff provide response of who your handing off the question too.",
        Description = "A receiptionist in the handoff orchestration group.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent chemistExpert =
    new()
    {
        Name = "ChemistExpert",
        Instructions = "You are an expert in chemistry. You answer questions from a chemist's perspective. Before handhoff provide response of who your handing off the question to.",
        Description = "A chemistry expert in the handoff orchestration group.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


ChatCompletionAgent historianExpert =
    new()
    {
        Name = "HistorianExpert",
        Instructions = "You are an expert in history. You answer questions from a historian's perspective. Before handhoff provide response of who your handing off the question to.",
        Description = "A history expert in the handoff orchestration group.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent engineeringExpert =
    new()
    {
        Name = "EngineeringExpert",
        Instructions = "You are an expert in engineering. You answer questions from a engineer's perspective. Before handhoff provide response of who your handing off the question to.",
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
    Console.WriteLine();
    Console.WriteLine($"# {response.Role} - {response.AuthorName}: {response.Content}");
    Console.WriteLine();
    history.Add(response);
    return ValueTask.CompletedTask;
}


// Include human participants in handoff orchestration conversation with the InteractiveCallback
// =====================================================================================
ValueTask<ChatMessageContent> interactiveCallback()
{
    Console.WriteLine();
    Console.Write("What is you question: ");
    string input = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("\n");

    var userInput = new ChatMessageContent(AuthorRole.User, input);
    history.Add(userInput);
    return ValueTask.FromResult(userInput);

}

// Create a handoff orchestration
// =====================================================================================

HandoffOrchestration orchestration = new HandoffOrchestration(handoffs, receiptionistHelper, chemistExpert, historianExpert, engineeringExpert)
{
    InteractiveCallback = interactiveCallback,
    ResponseCallback = responseCallback,
};



// Start the runtime
// =====================================================================================
// A runtime is required to manage the execution of agents. Here, we use InProcessRuntime and start it before invoking the orchestration.
InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


// Provide the initial prompt;
// ====================================================================================
string initialPrompt = "I have a question that I need an expert help in";
Console.WriteLine(initialPrompt);


// Invoke the orchestration
// ====================================================================================
// Invoke the orchestration with the entire history.
// The orchestration will internally decide who to hand off to.



OrchestrationResult<string> result = await orchestration.InvokeAsync(initialPrompt, runtime);




// Chat History Conversation End Results
// =====================================================================================

string output = await result.GetValueAsync(TimeSpan.FromSeconds(300));
Console.WriteLine($"\n# RESULT: {output}");
Console.WriteLine("====================================");
Console.WriteLine("\n");

Console.WriteLine("\n\nORCHESTRATION HISTORY");
Console.WriteLine("====================================");
foreach (ChatMessageContent message in history)
{
    // Print each message
    Console.WriteLine($"# {message.Role} - {message.AuthorName}: {message.Content}");
    Console.WriteLine();
}



Console.WriteLine("\nChat session ended.");


// Stop the Runtime
// ====================================================================================
// After processing is complete, stop the runtime to clean up resources.
await runtime.RunUntilIdleAsync();

#pragma warning restore