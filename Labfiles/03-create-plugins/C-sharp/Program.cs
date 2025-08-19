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
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

var kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add the plugin to the kernel
kernel.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");

// Configure function choice behavior
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};


var history = new ChatHistory();
history.AddSystemMessage("The year is 2025 and the current month is January");

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

void AddUserMessage(string msg) {
    Console.WriteLine("User: " + msg);
    history.AddUserMessage(msg);
}