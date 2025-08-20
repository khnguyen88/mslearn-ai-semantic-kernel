using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Agents;

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


// Import plugins to the kernel (Comment Out)
// ---------------------------------------------------------------
//kernel.ImportPluginFromType<DevopsPlugin>();

// Create prompt execution settings
// ---------------------------------------------------------------
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create chat history
// ---------------------------------------------------------------
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chatHistory = [];

// Create a kernel function to deploy the staging environment
// ---------------------------------------------------------------
var deployStageFunction = kernel.CreateFunctionFromPrompt(
promptTemplate: @"This is the most recent build log:
    

 If there are errors, do not deploy the stage environment. Otherwise, invoke the stage deployment function",
functionName: "DeployStageEnvironment",
description: "Deploy the staging environment"
);

//kernel.Plugins.AddFromFunctions("DeployStageEnvironment", [deployStageFunction]);

// Create a handlebars prompt
// ---------------------------------------------------------------
string hbprompt = """
     <message role="system">Instructions: Before creating a new branch for a user, request the new branch name and base branch name/message>
     <message role="user">Can you create a new branch?</message>
     <message role="assistant">Sure, what would you like to name your branch? And which base branch would you like to use?</message>
     <message role="user"></message>
     <message role="assistant">
     """;

// Create the prompt template config using handlebars format
// ---------------------------------------------------------------
var templateFactory = new HandlebarsPromptTemplateFactory();
var promptTemplateConfig = new PromptTemplateConfig()
{
    Template = hbprompt,
    TemplateFormat = "handlebars",
    Name = "CreateBranch",
};

// Create a plugin function from the prompt
// ---------------------------------------------------------------
var promptFunction = kernel.CreateFunctionFromPrompt(promptTemplateConfig, templateFactory);
var branchPlugin = kernel.CreatePluginFromFunctions("BranchPlugin", [promptFunction]);

//kernel.Plugins.Add(branchPlugin);


// Create Chat Completion Agent (Extra)
// ===================================================================
ChatCompletionAgent devOpAgent =
    new()
    {
        Name = "DevOpAgent",
        Instructions = "You are an AI DevOp Agents. Assist the users in anyway you can based on their request. Use the Plugin methods provided to help you.",
        Description = "An agent that assists the users on Devop Task given the Plugin methods and present them in human readable format.",
        Kernel = kernel,
        Arguments = new KernelArguments(openAIPromptExecutionSettings)
    };


// Provide DevOp Agents with tools (Extra)
// ===================================================================
devOpAgent.Kernel.Plugins.Add(branchPlugin);
devOpAgent.Kernel.Plugins.AddFromFunctions("DeployStageEnvironment", [deployStageFunction]);
devOpAgent.Kernel.ImportPluginFromType<DevopsPlugin>();


// Add filters to the kernel
// ---------------------------------------------------------------
kernel.FunctionInvocationFilters.Add(new PermissionFilter());

//Console.WriteLine("Press enter to exit");
//Console.WriteLine("Assistant: How may I help you?");
//Console.Write("User: ");

//string input = Console.ReadLine()!;

// User interaction logic
// ---------------------------------------------------------------
//while (input != "")
//{
//    chatHistory.AddUserMessage(input);
//    await GetReply();
//    input = GetInput();
//}

Console.WriteLine("DevOp Demo - Type 'EXIT' to end\n");
Console.WriteLine("Assistant: How may I help you?");
ChatHistoryAgentThread agentThread = new(); // Manages conversation state
bool isComplete = false;
do
{
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    if (string.IsNullOrWhiteSpace(input)) { continue; }

    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }
    var message = new ChatMessageContent(AuthorRole.User, input); //Add user message to conversation

    await foreach (StreamingChatMessageContent response in devOpAgent.InvokeStreamingAsync(message, agentThread))
    {
        Console.Write($"{response.Content}"); // Stream to console
    }
    Console.WriteLine("");

} while (!isComplete);
await agentThread.DeleteAsync(); // Delete the thread when it is no longer needed
Console.WriteLine("\nChat session ended.");

//string GetInput()
//{
//    Console.Write("User: ");
//    string input = Console.ReadLine()!;
//    chatHistory.AddUserMessage(input);
//    return input;
//}

//async Task GetReply()
//{
//    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
//        chatHistory,
//        executionSettings: openAIPromptExecutionSettings,
//        kernel: kernel
//    );
//    Console.WriteLine("Assistant: " + reply.ToString());
//    chatHistory.AddAssistantMessage(reply.ToString());
//}

class DevopsPlugin
{
    // Create a kernel function to build the stage environment
    [KernelFunction("BuildStageEnvironment")]
    string BuildStageEnvironment()
    {
        return "Stage build completed.";
    }

    [KernelFunction("DeployToStage")]
    public string DeployToStage()
    {
        return "Staging site deployed successfully.";
    }

    [KernelFunction("DeployToProd")]
    public string DeployToProd() 
    {
        return "Production site deployed successfully.";
    }

    [KernelFunction("CreateNewBranch")]
    public string CreateNewBranch(string branchName, string baseBranch) {
        return $"Created new branch `{branchName}` from `{baseBranch}`";
    }

    [KernelFunction("ReadLogFile")]
    public string ReadLogFile() 
    {
        string content = File.ReadAllText($"Files/build.log");
        return content;
    }
}

// Create a function filter
class PermissionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Check the plugin and function names
        if ((context.Function.PluginName == "DevopsPlugin" && context.Function.Name == "DeployToProd"))
        {
            // Request user approval
            Console.WriteLine("System Message: The assistant requires an approval to complete this operation. Do you approve (Y/N)");
            Console.Write("User: ");
            string shouldProceed = Console.ReadLine()!;

            // Proceed if approved
            if (shouldProceed != "Y")
            {
                context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
                return;
            }
        }

        await next(context);
    }
}