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
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

var kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add the plugin to the kernel
// ---------------------------------------------------------------
kernel.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");

// Configure function choice behavior
// ---------------------------------------------------------------

//Option 1 - Let OpenAI decide which Functions to use for the plugin.
//OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
//{
//    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
//};

//Option 2 - We specify the functions we when the plugins to use. Note here we only provide the search kernel function and not the book flight. So our plugin does not have the capabilities.
KernelFunction searchFlights = kernel.Plugins.GetFunction("FlightBookingPlugin", "search_flights");

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions: [searchFlights]),
};

var history = new ChatHistory();
history.AddSystemMessage("The year is 2025 and the current month is January");

AddUserMessage("Find me a flight to Tokyo on the 19");

//Just add to narrow down repsponse
history.AddSystemMessage("If the assistant does not have the ability to book. Just specify that it does not have the capbilities to book.");
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