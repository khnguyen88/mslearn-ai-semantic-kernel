using Microsoft.Extensions.Configuration;

// Import namespaces
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


// Set your values in appsettings.json

//string filePath = Path.GetFullPath("appsettings.json");
//var config = new ConfigurationBuilder()
//    .AddJsonFile(filePath)
//    .Build();

var config = new ConfigurationBuilder().AddUserSecrets("968d9b73-5aa8-4f11-b52d-51e77bd74408").Build();

string modelId = config["AzureAIFoundry:AIModel:Name"]!;
string endpoint = config["AzureAIFoundry:AIModel:Uri"]!;
string apiKey = config["AzureAIFoundry:AIModel:ApiKey"]!;


// Create a kernel with Azure OpenAI chat completion
var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);


// Build the kernel
Kernel kernel = kernelBuilder.Build();

// Test the chat completion service
var result = await kernel.InvokePromptAsync("Give me a list of 10 breakfast foods with eggs and cheese");
Console.WriteLine(result);