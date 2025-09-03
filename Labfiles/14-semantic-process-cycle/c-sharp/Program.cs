using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Process;
using Microsoft.SemanticKernel.Process.Runtime;
using Microsoft.SemanticKernel.Process.Tools;
using Microsoft.VisualBasic;
using new_sk_labs.Plugins;
using new_sk_labs.Steps;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

Console.Clear();

#pragma warning disable
// Install the Semantic Kernel Process Framework Local Runtime package
//dotnet add package Microsoft.SemanticKernel.Process.LocalRuntime --version 1.46.0-alpha
// and
// Install the Semantic Kernel Process Framework Dapr Runtime package
//dotnet add package Microsoft.SemanticKernel.Process.Runtime.Dapr --version 1.46.0-alpha

// Obtain your API access information
// =====================================================================================
//string filePath = Path.GetFullPath("appsettings.json");
//var config = new ConfigurationBuilder()
//    .AddJsonFile(filePath)
//    .Build();

var config = new ConfigurationBuilder().AddUserSecrets("d8159b05-7d55-4374-8785-8a023929f4ce").Build();


// Set your values in appsettings.json (not used in this example)
// =====================================================================================
//string apiKey = config["PROJECT_KEY"]!;
//string endpoint = config["PROJECT_ENDPOINT"]!;
//string deploymentName = config["DEPLOYMENT_NAME"]!;


// Set your values in secret.json;
// =====================================================================================
string deploymentName = config["AzureAIFoundry:AIModel:Name"]!;
string endpoint = config["AzureAIFoundry:AIModel:Uri"]!;
string apiKey = config["AzureAIFoundry:AIModel:ApiKey"]!;


// Create a kernel builder with Azure OpenAI chat completion
// =====================================================================================
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);


// Create a simple kernel 
// =====================================================================================
var kernel = kernelBuilder.Build();


// Optional: Create prompt execution settings
// =====================================================================================
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};


try 
{
    // Create a new Semantic Kernel process
    // =====================================================================================
    ProcessBuilder processBuilder = new("ContentCreationWorkflow");


    // Add the processing steps
    // =====================================================================================
    var requestContentStep = processBuilder.AddStepFromType<RequestContentStep>();
    var writeContentStep = processBuilder.AddStepFromType<WriteContentStep>();
    var publishContentStep = processBuilder.AddStepFromType<PublishContentStep>();


    // Define the process flow
    // ====================================================================================
    processBuilder
        .OnInputEvent(ProcessEvents.Start)
        .SendEventTo(new ProcessFunctionTargetBuilder(
            requestContentStep,
            functionName: RequestContentStep.Functions.RequestContent
            ));

    requestContentStep
        .OnFunctionResult()
        .SendEventTo(
            new ProcessFunctionTargetBuilder(
                writeContentStep,
                functionName: WriteContentStep.Functions.WriteContent,
                parameterName: "request"
            ));

    writeContentStep
        .OnFunctionResult()
        .SendEventTo(
            new ProcessFunctionTargetBuilder(
                publishContentStep,
                functionName: PublishContentStep.Functions.PublishContent,
                parameterName: "content"));

    publishContentStep
        .OnFunctionResult()
        .StopProcess();


    // Build the process to get a handle that can be started
    // ====================================================================================
    KernelProcess kernelProcess = processBuilder.Build();


    // Generate a Mermaid diagram for the process and print it to the console (OPTIONAL)
    // ====================================================================================
    string mermaidGraph = kernelProcess.ToMermaid();
    Console.WriteLine();
    Console.WriteLine($"=== Start - Mermaid Diagram for '{processBuilder.Name}' ===");
    Console.WriteLine(mermaidGraph);
    Console.WriteLine($"=== End - Mermaid Diagram for '{processBuilder.Name}' ===");
    Console.WriteLine();


    // Start the process with an initial external event
    // ====================================================================================
    await using var initialProcessResult = await kernelProcess.StartAsync(
        kernel,
        new KernelProcessEvent()
        {
            Id = ProcessEvents.Start,
            Data = null
        });

    Console.WriteLine("Initial Process Results: ");
    Console.WriteLine(JsonSerializer.Serialize(initialProcessResult));
    Console.WriteLine();

    var finalState = await initialProcessResult.GetStateAsync();
    var finalCompletion = finalState.ToProcessStateMetadata();

    Console.WriteLine("");
    Console.WriteLine(JsonSerializer.Serialize(finalCompletion));

} catch (Exception ex)
{
    Console.WriteLine(ex);
}
#pragma warning restore


public static class ProcessEvents
{
    public const string Start = nameof(Start);
}




//// Create a process that will interact with the chat completion service
//ProcessBuilder process = new("ChatBot");
//var introStep = process.AddStepFromType<IntroStep>();
//var userInputStep = process.AddStepFromType<ChatUserInputStep>();
//var responseStep = process.AddStepFromType<ChatBotResponseStep>();

//// Define the behavior when the process receives an external event
//process
//    .OnInputEvent(ChatBotEvents.StartProcess)
//    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

//// When the intro is complete, notify the userInput step
//introStep
//    .OnFunctionResult()
//    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

//// When the userInput step emits an exit event, send it to the end step
//userInputStep
//    .OnEvent(ChatBotEvents.Exit)
//    .StopProcess();

//// When the userInput step emits a user input event, send it to the assistantResponse step
//userInputStep
//    .OnEvent(CommonEvents.UserInputReceived)
//    .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));

//// When the assistantResponse step emits a response, send it to the userInput step
//responseStep
//    .OnEvent(ChatBotEvents.AssistantResponseGenerated)
//    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

//// Build the process to get a handle that can be started
//KernelProcess kernelProcess = process.Build();

//// Generate a Mermaid diagram for the process and print it to the console
//string mermaidGraph2 = kernelProcess.ToMermaid();
//Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
//Console.WriteLine(mermaidGraph2);
//Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

//// Start the process with an initial external event
//await using var runningProcess = await kernelProcess.StartAsync(kernel, new KernelProcessEvent() { Id = ChatBotEvents.StartProcess, Data = null });



///// <summary>
///// The simplest implementation of a process step. IntroStep
///// </summary>
//public sealed class IntroStep : KernelProcessStep
//{
//    /// <summary>
//    /// Prints an introduction message to the console.
//    /// </summary>
//    [KernelFunction]
//    public void PrintIntroMessage()
//    {
//        System.Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
//    }
//}

///// <summary>
///// A step that elicits user input.
///// </summary>
//public sealed class ChatUserInputStep : ScriptedUserInputStep
//{
//    public override void PopulateUserInputs(UserInputState state)
//    {
//        state.UserInputs.Add("Hello");
//        state.UserInputs.Add("How tall is the tallest mountain?");
//        state.UserInputs.Add("How low is the lowest valley?");
//        state.UserInputs.Add("How wide is the widest river?");
//        state.UserInputs.Add("exit");
//        state.UserInputs.Add("This text will be ignored because exit process condition was already met at this point.");
//    }

//    public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
//    {
//        var userMessage = this.GetNextUserMessage();

//        if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
//        {
//            // exit condition met, emitting exit event
//            await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit, Data = userMessage });
//            return;
//        }

//        // emitting userInputReceived event
//        await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = userMessage });
//    }
//}

///// <summary>
///// A step that takes the user input from a previous step and generates a response from the chat completion service.
///// </summary>
//public sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
//{
//    public static class ProcessFunctions
//    {
//        public const string GetChatResponse = nameof(GetChatResponse);
//    }

//    /// <summary>
//    /// The internal state object for the chat bot response step.
//    /// </summary>
//    internal ChatBotState? _state;

//    /// <summary>
//    /// ActivateAsync is the place to initialize the state object for the step.
//    /// </summary>
//    /// <param name="state">An instance of <see cref="ChatBotState"/></param>
//    /// <returns>A <see cref="ValueTask"/></returns>
//    public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
//    {
//        _state = state.State;
//        return ValueTask.CompletedTask;
//    }

//    /// <summary>
//    /// Generates a response from the chat completion service.
//    /// </summary>
//    /// <param name="context">The context for the current step and process. <see cref="KernelProcessStepContext"/></param>
//    /// <param name="userMessage">The user message from a previous step.</param>
//    /// <param name="_kernel">A <see cref="Kernel"/> instance.</param>
//    /// <returns></returns>
//    [KernelFunction(ProcessFunctions.GetChatResponse)]
//    public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
//    {
//        _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
//        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
//        ChatMessageContent response = await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false);
//        if (response == null)
//        {
//            throw new InvalidOperationException("Failed to get a response from the chat completion service.");
//        }

//        System.Console.ForegroundColor = ConsoleColor.Yellow;
//        System.Console.WriteLine($"ASSISTANT: {response.Content}");
//        System.Console.ResetColor();

//        // Update state with the response
//        _state.ChatMessages.Add(response);

//        // emit event: assistantResponse
//        await context.EmitEventAsync(new KernelProcessEvent { Id = ChatBotEvents.AssistantResponseGenerated, Data = response });
//    }
//}

///// <summary>
///// The state object for the <see cref="ChatBotResponseStep"/>.
///// </summary>
//public sealed class ChatBotState
//{
//    internal ChatHistory ChatMessages { get; } = new();
//}

///// <summary>
///// A class that defines the events that can be emitted by the chat bot process. This is
///// not required but used to ensure that the event names are consistent.
///// </summary>
//public static class ChatBotEvents
//{
//    public const string StartProcess = "startProcess";
//    public const string IntroComplete = "introComplete";
//    public const string AssistantResponseGenerated = "assistantResponseGenerated";
//    public const string Exit = "exit";
//}

//public class ScriptedUserInputStep : KernelProcessStep<UserInputState>
//{
//    public static class ProcessStepFunctions
//    {
//        public const string GetUserInput = nameof(GetUserInput);
//    }

//    protected bool SuppressOutput { get; init; }

//    /// <summary>
//    /// The state object for the user input step. This object holds the user inputs and the current input index.
//    /// </summary>
//    private UserInputState? _state;

//    /// <summary>
//    /// Method to be overridden by the user to populate with custom user messages
//    /// </summary>
//    /// <param name="state">The initialized state object for the step.</param>
//    public virtual void PopulateUserInputs(UserInputState state)
//    {
//        return;
//    }

//    /// <summary>
//    /// Activates the user input step by initializing the state object. This method is called when the process is started
//    /// and before any of the KernelFunctions are invoked.
//    /// </summary>
//    /// <param name="state">The state object for the step.</param>
//    /// <returns>A <see cref="ValueTask"/></returns>
//    public override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)
//    {
//        _state = state.State;

//        PopulateUserInputs(_state!);

//        return ValueTask.CompletedTask;
//    }

//    internal string GetNextUserMessage()
//    {
//        if (_state != null && _state.CurrentInputIndex >= 0 && _state.CurrentInputIndex < this._state.UserInputs.Count)
//        {
//            var userMessage = this._state!.UserInputs[_state.CurrentInputIndex];
//            _state.CurrentInputIndex++;

//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"USER: {userMessage}");
//            Console.ResetColor();

//            return userMessage;
//        }

//        Console.WriteLine("SCRIPTED_USER_INPUT: No more scripted user messages defined, returning empty string as user message");
//        return string.Empty;
//    }

//    /// <summary>
//    /// Gets the user input.
//    /// Could be overridden to customize the output events to be emitted
//    /// </summary>
//    /// <param name="context">An instance of <see cref="KernelProcessStepContext"/> which can be
//    /// used to emit events from within a KernelFunction.</param>
//    /// <returns>A <see cref="ValueTask"/></returns>
//    [KernelFunction(ProcessStepFunctions.GetUserInput)]
//    public virtual async ValueTask GetUserInputAsync(KernelProcessStepContext context)
//    {
//        var userMessage = this.GetNextUserMessage();
//        // Emit the user input
//        if (string.IsNullOrEmpty(userMessage))
//        {
//            await context.EmitEventAsync(new() { Id = CommonEvents.Exit });
//            return;
//        }

//        await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = userMessage });
//    }
//}

///// <summary>
///// The state object for the <see cref="ScriptedUserInputStep"/>
///// </summary>
//public record UserInputState
//{
//    public List<string> UserInputs { get; init; } = [];

//    public int CurrentInputIndex { get; set; } = 0;
//}

//public static class CommonEvents
//{
//    public static readonly string UserInputReceived = nameof(UserInputReceived);
//    public static readonly string UserInputComplete = nameof(UserInputComplete);
//    public static readonly string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
//    public static readonly string Exit = nameof(Exit);
//}

