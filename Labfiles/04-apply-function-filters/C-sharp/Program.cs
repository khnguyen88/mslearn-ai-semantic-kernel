using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Obtain your API access information
// ---------------------------------------------------------------
//string filePath = Path.GetFullPath("appsettings.json");
//var config = new ConfigurationBuilder()
//    .AddJsonFile(filePath)
//    .Build();

var config = new ConfigurationBuilder().AddUserSecrets("aaac160b-bb37-4105-968e-78510b8a57f4").Build();


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

// Create a kernel with Azure OpenAI chat completion
// ---------------------------------------------------------------
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

var kernel = builder.Build();
kernel.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");

// Add the permission filter to the kernel
// ---------------------------------------------------------------
kernel.FunctionInvocationFilters.Add(new PermissionFilter());

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

history.AddSystemMessage("Assume the current date is January 1 2025");
AddUserMessage("Find me a flight to Tokyo on the 19");
await GetReply();
GetInput();
await GetReply();

void GetInput() {
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    history.AddUserMessage(input);
}

async Task GetReply() {
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    history.AddAssistantMessage(reply.ToString());
}

void AddUserMessage(string msg)
{
    Console.WriteLine("User: " + msg);
    history.AddUserMessage(msg);
}

// Create the function filer class
// ---------------------------------------------------------------

public class PermissionFilter : IFunctionInvocationFilter
{

    // Implement the function invocation method
    // ---------------------------------------------------------------
    public async Task OnFunctionInvocationAsync(Microsoft.SemanticKernel.FunctionInvocationContext context, Func<Microsoft.SemanticKernel.FunctionInvocationContext, Task> next)
    {
        if (!HasUserPermission(context.Function.PluginName, context.Function.Name))
        {
            context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
            return;
        }

        await next(context);
    }

    private Boolean HasUserPermission(string pluginName, string functionName)
    {
        if (pluginName.Equals("FlightBookingPlugin") && functionName.Equals("book_flight"))
        {
            Console.WriteLine("System Message: The agent requires an approval to complete this operation. Do you approve (Y/N)");
            Console.Write("User: ");
            string shouldProceed = Console.ReadLine()!;

            if (shouldProceed != "Y")
            {
                return false;
            }
        }

        return true;
    }
}