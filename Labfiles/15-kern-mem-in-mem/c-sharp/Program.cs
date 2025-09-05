using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using OpenAI;
using OpenAI.Embeddings;
using Spectre.Console;
using System;
using System.ComponentModel;
using TiktokenTokenizer = Microsoft.ML.Tokenizers.TiktokenTokenizer;

// Install packages for Kernel Memory
// dotnet add package Microsoft.Extensions.Configuration.Json
// dotnet add package microsoft.kernelmemory ////Probably should have just installed this first
// dotnet add package microsoft.kernelmemory.core
// dotnet add package microsoft.kernelmemory.ai.azureopenai
// dotnet add package Microsoft.KernelMemory.MemoryDb.AzureAISearch
// dotnet add package Spectre.Console
// dotnet add package Microsoft.KernelMemory.AI.OpenAI
// dotnet add package Microsoft.ML.Tokenizers
// dotnet add package Microsoft.ML.Tokenizers.Data.O200kBase --version 1.0.2

// Install packages for Agent Orchestration
// https://github.com/microsoft/semantic-kernel/issues/12453
// dotnet add package Microsoft.SemanticKernel.Agents.Runtime.InProcess --prerelease
// dotnet add package Microsoft.SemanticKernel.Agents.Orchestration --prerelease
// dotnet add package Microsoft.SemanticKernel.Agents.Magentic --prerelease

// Kernel Memory
// Requires a LLM Model - In this example we used the deployed "gpt-4o-mini" model from Azure. Used to ask questions and retrieve answers.
// Requires a Embedding-Model - In this example we used the deployed "text-embedding-3-small" model from Azure. Used to index and store resources such as, documents and queries, inside of a vector store (database)

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
string textDeploymentName = config["AzureAIFoundry:AIModel:Name"]!;
string endpoint = config["AzureAIFoundry:AIModel:Uri"]!;
string apiKey = config["AzureAIFoundry:AIModel:ApiKey"]!;

string embeddingDeploymentName = config["AzureAIFoundry:AIEmbedding:Name"]!;
string embeddingEndpoint = config["AzureAIFoundry:AIEmbedding:Uri"]!;
string embeddingApiKey = config["AzureAIFoundry:AIEmbedding:ApiKey"]!;

string searchEndpoint = config["AzureAIFoundry:AISearch:Uri"]!;
string searchApiKey = config["AzureAIFoundry:AISearch:ApiKey"]!;


// Create a kernel builder and kernel with Azure OpenAI chat completion
// =====================================================================================
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(textDeploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();


// TESTING EMBEDDING MODEL ACCESS (SAMPLE CODE)
// =====================================================================================
//var embeddingEndpoint2 = new Uri(embeddingEndpoint);
//var credential = new AzureKeyCredential(embeddingApiKey);
//var model = embeddingDeploymentName;
//var deploymentName = embeddingDeploymentName;

//var openAIOptions = new OpenAIClientOptions()
//{
//    Endpoint = embeddingEndpoint2
//};

//var client = new EmbeddingClient(embeddingDeploymentName, credential, openAIOptions);

//OpenAIEmbeddingCollection response = client.GenerateEmbeddings(
//    new List<string> { "first phrase", "second phrase", "third phrase" }
//);

//foreach (OpenAIEmbedding embedding in response)
//{
//    ReadOnlyMemory<float> vector = embedding.ToFloats();
//    int length = vector.Length;
//    System.Console.Write($"data[{embedding.Index}]: length={length}, ");
//    System.Console.Write($"[{vector.Span[0]}, {vector.Span[1]}, ..., ");
//    System.Console.WriteLine($"{vector.Span[length - 2]}, {vector.Span[length - 1]}]");
//}


// Create a kernel memory builder and memory with Azure OpenAI
// =====================================================================================

//IKernelMemoryBuilder memoryBuilder = new KernelMemoryBuilder()
//    .WithAzureOpenAITextGeneration(new AzureOpenAIConfig { APIKey = apiKey, Endpoint = endpoint, Deployment = textDeploymentName })
//    .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig { APIKey = apiKey, Endpoint = embeddingEndpoint, Deployment = embeddingDeploymentName });
/*  .WithAzureAISearchMemoryDb(new AzureAISearchConfig { APIKey = searchApiKey, Endpoint = searchEndpoint })*/

OpenAIConfig openAiConfig = new()
{
    APIKey = apiKey,
    Endpoint = endpoint,
    EmbeddingModel = embeddingDeploymentName,
    EmbeddingModelTokenizer = "o200k", //Used by chat gpt
    TextModel = textDeploymentName,
    TextModelTokenizer = "o200k"
};

AzureAISearchConfig azureAISearchConfig = new()
{
    APIKey = searchApiKey,
    Endpoint = searchEndpoint,
    Auth = AzureAISearchConfig.AuthTypes.APIKey
};

IKernelMemoryBuilder memoryBuilder = new KernelMemoryBuilder()
    .WithOpenAI(openAiConfig);

IKernelMemory memory = memoryBuilder.Build(); //Temp to resolve warning. Need to investigate a bit more


//NOTE: Further research is required below: https://github.com/microsoft/kernel-memory/issues/203
//IKernelMemoryBuilder memoryBuilder = new KernelMemoryBuilder()
//    .WithOpenAI(openAiConfig)
//    .WithAzureAISearchMemoryDb(azureAISearchConfig);

//IKernelMemory memory = memoryBuilder.Build(new KernelMemoryBuilderBuildOptions { AllowMixingVolatileAndPersistentData = true }); //Temp to resolve warning. Need to investigate a bit more

// Feed the kernel memory documents and texts
// =====================================================================================

await memory.ImportTextAsync(text: "Khiem had a cat name Benzi. She was a calico", tags: new TagCollection { "khiem", "benzi" });
await memory.ImportTextAsync("Tom The Terrible Train was not a train but a tank.");
await memory.ImportDocumentAsync("facts.txt", "matt-eland-repository-facts");
await memory.ImportWebPageAsync(url: "https://en.wikipedia.org/wiki/Dinosaur", documentId: "wiki-dinosaur");
await memory.ImportWebPageAsync(url: "https://github.com/microsoft/kernel-memory/tree/main", documentId: "microsoft-github-kernelmemory-page", tags: new TagCollection { "github", "kernel-memory" });
await memory.ImportWebPageAsync(url: "https://github.com/IntegerMan/DocumentSearchWithKernelMemory/blob/main/MattEland.Demos.KernelMemory.DocumentSearch/MattEland.Demos.KernelMemory.DocumentSearch/Program.cs", documentId: "matt-eland-kernel-memory-search-code", new TagCollection { "github", "kernel-memory", "matt-eland" });
await memory.ImportWebPageAsync(url: "https://blog.leadingedje.com/post/ai/documents/kernelmemory.html", documentId: "leadingEDJE-kernel-memory-search-blog", new TagCollection { "github", "kernel-memory", "leadingEDJE" });


// Search Kernel Memory API
// =====================================================================================
SearchResult results = await memory.SearchAsync("Did Khiem have a cat?");

Table table = new Table()
    .AddColumns("Document", "Partition", "Section", "Score", "Text");
foreach (var citation in results.Results)
{
    foreach (var part in citation.Partitions)
    {
        string snippet = part.Text;
        if (part.Text.Length > 100)
        {
            snippet = part.Text[..100] + "...";
        }

        table.AddRow(citation.DocumentId, part.PartitionNumber.ToString(), part.SectionNumber.ToString(), part.Relevance.ToString("P2"), snippet);
    }
}

table.Expand();
Console.Write(table);
Console.WriteLine();


// Ask Kernel Memory API
// =====================================================================================
// Ask me a question and I shall respond
MemoryAnswer answer = await memory.AskAsync("Did Khiem have a cat? If so what was her name?");

Console.WriteLine(answer.Result);


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
Console.WriteLine("What should we write a story about?");
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
