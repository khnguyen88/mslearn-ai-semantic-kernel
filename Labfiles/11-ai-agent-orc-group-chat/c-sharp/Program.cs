using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Security.Cryptography;

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

ChatCompletionAgent physicistExpert =
    new()
    {
        Name = "PhysicistExpert",
        Instructions = "You are an expert in physics. You answer questions from a physicist's perspective.",
        Description = "A physics expert in a group chat orchestration.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent chemistExpert =
    new()
    {
        Name = "ChemistExpert",
        Instructions = "You are an expert in chemistry. You answer questions from a chemist's perspective.",
        Description = "A chemistry expert in a group chat orchestration.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


ChatCompletionAgent historianExpert =
    new()
    {
        Name = "HistorianExpert",
        Instructions = "You are an expert in history. You answer questions from a historian's perspective.",
        Description = "A history expert in a group chat orchestration.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent engineeringExpert =
    new()
    {
        Name = "EngineeringExpert",
        Instructions = "You are an expert in engineering. You answer questions from a engineer's perspective.",
        Description = "An engineering expert in a group chat orchestration.",
        Kernel = kernel.Clone(),
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };

ChatCompletionAgent chatModerator =
    new()
    {
        Name = "ChatGroupModerator",
        Instructions = "You are the group moderator, after each feedback, you will ask the user if the have another interactive question? Ask the user to type 'E X I T' to leave",
        Description = "A chemistry expert in a group chat orchestration.",
        Kernel = kernel,
        //Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


// Manages chat history and develop callback to caputre agent responses
// =====================================================================================
// You can create a callback to capture agent responses as the conversation progresses via the ResponseCallback property.

ChatHistory history = [];

ValueTask ResponseCallback(ChatMessageContent response)
{
    Console.WriteLine();
    Console.WriteLine($"# {response.Role} - {response.AuthorName}: {response.Content}");
    Console.WriteLine();
    history.Add(response);
    return ValueTask.CompletedTask;
}


// Include human participants in group chat orchestration conversation with the InteractiveCallback
// =====================================================================================
ValueTask<ChatMessageContent> InteractiveCallback()
{
    Console.WriteLine();
    Console.Write("What is your question: ");
    string input = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("\n");

    var userInput = new ChatMessageContent(AuthorRole.User, input);
    history.Add(userInput);
    return ValueTask.FromResult(userInput);

}

// Create a group chat manager
// =====================================================================================
CustomInteractiveGroupChatManager manager = new CustomInteractiveGroupChatManager
{
    MaximumInvocationCount = 10,
    InteractiveCallback = InteractiveCallback,
};


// Create a group chat orchestration
// =====================================================================================
GroupChatOrchestration orchestration = new GroupChatOrchestration(
    manager,
    physicistExpert,
    chemistExpert,
    historianExpert,
    engineeringExpert,
    chatModerator)
{
    ResponseCallback = ResponseCallback,
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



OrchestrationResult<string> result = await orchestration.InvokeAsync(initialPrompt, runtime);




// Chat History Conversation End Results
// =====================================================================================

//string output = await result.GetValueAsync(TimeSpan.FromSeconds(300));
string output = await result.GetValueAsync();
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



// Note: Managers have some methods that can dictate the group chat

//* ShouldRequestUserInput: Checks if user(human) input is required before the next agent speaks. If true, the orchestration pauses for user input. The user input is then added to the chat history of the manager and sent to all agents.
//* ShouldTerminate: Determines if the group chat should end (for example, if a maximum number of rounds is reached or a custom condition is met). If true, the orchestration proceeds to result filtering.
//* FilterResults: Called only if the chat is terminating, to summarize or process the final results of the conversation.
//* SelectNextAgent: If the chat is not terminating, selects the next agent to respond in the conversation.

#pragma warning disable
sealed class CustomInteractiveGroupChatManager : RoundRobinGroupChatManager
{
    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // This is the custom logic. You can change it to fit your needs.
        // For example, this will request user input if the last message content
        // contains the word "interactive" or "human input".
        bool shouldRequest =
            (history.LastOrDefault()?.Content?.Contains("question", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (history.LastOrDefault()?.Content?.Contains("interactive", StringComparison.OrdinalIgnoreCase) ?? false);

        if (shouldRequest)
        {
            return ValueTask.FromResult(new GroupChatManagerResult<bool>(true)
            {
                Reason = "The conversation requires human intervention."
            });
        }

        // If your custom logic doesn't trigger, the default behavior is to not request input.
        return ValueTask.FromResult(new GroupChatManagerResult<bool>(false)
        {
            Reason = "No user input required."
        });


        // Returning ValueTask.FromResult(new GroupChatManagerResult<bool>(true) prompt user interaction after an individual agent response. Behavior is different.
    }

    public override ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // This is the custom logic. You can change it to fit your needs.
        bool shouldTerminate = history.LastOrDefault()?.Content?.Contains("EXIT", StringComparison.OrdinalIgnoreCase) ?? false;

        if (shouldTerminate)
        {
            return ValueTask.FromResult(new GroupChatManagerResult<bool>(true)
            {
                Reason = "The conversation should be terminated."
            });
        }

        // If your custom logic doesn't trigger, the default behavior is to not request input.
        return ValueTask.FromResult(new GroupChatManagerResult<bool>(false)
        {
            Reason = "The conversation will still continue."
        });
    }
}
#pragma warning restore