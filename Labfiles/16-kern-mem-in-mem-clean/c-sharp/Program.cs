
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using System.Text.Json;
using TiktokenTokenizer = Microsoft.ML.Tokenizers.TiktokenTokenizer;

Console.Clear();

#pragma warning disable
// Install packages for Kernel Memory
// dotnet add package Microsoft.Extensions.Configuration.Json
// dotnet add package microsoft.kernelmemory ////Probably should have just installed this first
// dotnet add package microsoft.kernelmemory.core
// dotnet add package microsoft.kernelmemory.ai.azureopenai
// dotnet add package Microsoft.KernelMemory.MemoryDb.AzureAISearch
// dotnet add package Spectre.Console
// dotnet add package Microsoft.KernelMemory.AI.OpenAI


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


// Create a kernel memory builder and memory with Azure OpenAI
// =====================================================================================

AzureOpenAIConfig textAzureOpenAIConfig = new()
{
    APIKey = apiKey,
    Endpoint = endpoint,
    Tokenizer = "o200k",
    Deployment = textDeploymentName,
    APIType = AzureOpenAIConfig.APITypes.TextCompletion,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey

};
AzureOpenAIConfig embeddingAzureOpenAIConfig = new()
{
    APIKey = apiKey,
    Endpoint = endpoint,
    Tokenizer = "o200k",
    Deployment = embeddingDeploymentName,
    APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey
};

AzureAISearchConfig azureAISearchConfig = new()
{
    APIKey = searchApiKey,
    Endpoint = searchEndpoint,
    Auth = AzureAISearchConfig.AuthTypes.APIKey
};


IKernelMemoryBuilder memoryBuilder = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(textAzureOpenAIConfig)
    .WithAzureOpenAITextEmbeddingGeneration(embeddingAzureOpenAIConfig);

IKernelMemory memory = memoryBuilder.Build();


// Create a kernel builder with Azure OpenAI chat completion (Extra)
// ---------------------------------------------------------------
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(textDeploymentName, endpoint, apiKey);
var kernel = kernelBuilder.Build();


// Create Chat Completion Agents (Extra)
// =====================================================================================
ChatCompletionAgent devAgent =
    new()
    {
        Name = "CSharpDeveloperAgent",
        Instructions = "You a professional csharp and dotnet developer who specializes in Microsoft Semantic Kernel and Kernel Memory. Your task is to ingest the memory kernel results and complete and clean up the code.",
        Description = "A professional csharp or dot net developer",
        Kernel = kernel,
    };

// Create Chat Completion History (Extra)
// =====================================================================================
ChatHistoryAgentThread agentThread = new();

// Feed the kernel memory documents and texts
// =====================================================================================

var docId = await memory.ImportTextAsync(text: "Khiem had a cat name Benzi. She was a calico", tags: new TagCollection { "khiem", "benzi" });
//Console.WriteLine(JsonSerializer.Serialize(docId)); // Output the a generated documentId, if one has not been specified

await memory.ImportTextAsync("Tom The Terrible Train was not a train but a tank.");

await memory.ImportDocumentAsync("./docs/facts.txt", "matt-eland-repository-facts");

var docId2 = await memory.ImportWebPageAsync(url: "https://en.wikipedia.org/wiki/Dinosaur", documentId: "wiki-dinosaur");
//Console.WriteLine(JsonSerializer.Serialize(docId2)); // Output the documentId specified

await memory.ImportDocumentAsync("./docs/reddit-dinosaurs.txt", documentId: "reddit-dinosaur", tags: new TagCollection { "reddit" });

await memory.ImportWebPageAsync(url: "https://github.com/microsoft/kernel-memory/tree/main", documentId: "microsoft-github-kernelmemory-page", tags: new TagCollection { "github", "kernel-memory" });
await memory.ImportWebPageAsync(url: "https://github.com/IntegerMan/DocumentSearchWithKernelMemory/blob/main/MattEland.Demos.KernelMemory.DocumentSearch/MattEland.Demos.KernelMemory.DocumentSearch/Program.cs", documentId: "matt-eland-kernel-memory-search-code", new TagCollection { "github", "kernel-memory", "matt-eland" });
await memory.ImportWebPageAsync(url: "https://blog.leadingedje.com/post/ai/documents/kernelmemory.html", documentId: "leadingEDJE-kernel-memory-search-blog", new TagCollection { "github", "kernel-memory", "leadingEDJE", "blog" });
await memory.ImportWebPageAsync(url: "https://johnnyreilly.com/using-kernel-memory-to-chunk-documents-into-azure-ai-search", documentId: "johnny-reilly-kernel-memory-ai-search", new TagCollection { "github", "kernel-memory", "ai-search", "kernel-memory-ai-search", "john-reilly" });
await memory.ImportWebPageAsync(url: "https://dev.to/stormhub/kernel-memory-with-azure-openai-blob-storage-and-ai-search-services-1245", documentId: "dev-community-kernel-memory-ai-search-blob-storage", new TagCollection { "blog", "kernel-memory", "ai-search", "blob-storage", "kernel-memory-ai-search-blob-storage", "dev-community" });
await memory.ImportWebPageAsync(url: "https://github.com/StormHub/stormhub/blob/main/resources/2024-11-18/ConsoleApp/ConsoleApp/Program.cs", documentId: "storm-hub-kernel-memory-ai-search-blob-storage", new TagCollection { "github", "kernel-memory", "ai-search", "blob-storage", "kernel-memory-ai-search-blob-storage", "storm-hub" });

await memory.DeleteDocumentAsync("reddit-dinosaur"); //Allows you to delete data based on document Id


// Search Kernel Memory API
// =====================================================================================
string searchQuestion = "Did Khiem have a cat?";
Console.WriteLine(searchQuestion);
Console.WriteLine("----");
SearchResult results = await memory.SearchAsync(searchQuestion);
IAnsiConsole ansiConsole = AnsiConsole.Console;
Table table = new Table()
    .AddColumns("Document", "Partition", "Section", "Score", "Text");
foreach (var citation in results.Results)
{
    foreach (var part in citation.Partitions)
    {
        string snippet = part.Text;
        if (part.Text.Length > 15)
        {
            snippet = part.Text[..15] + "...";
        }
        table.AddRow(citation.DocumentId, part.PartitionNumber.ToString(), part.SectionNumber.ToString(), part.Relevance.ToString("P2"), snippet);
    }
}

table.Expand();
ansiConsole.Write(table);
ansiConsole.WriteLine();

// Ask Kernel Memory API
// =====================================================================================
string question = "Did Khiem have a cat? If so what was her name? I want just the name, no other information.";
Console.WriteLine(question);
Console.WriteLine("----");
MemoryAnswer answer = await memory.AskAsync(question);
Console.WriteLine(answer.Result);
Console.WriteLine();
printCitation(answer);

string question2 = "Who and what is Tom?";
Console.WriteLine(question2);
Console.WriteLine("----");
MemoryAnswer answer2 = await memory.AskAsync(question2);
Console.WriteLine(answer2.Result);
Console.WriteLine();
printCitation(answer2);

string question3 = "Did dinosaurs survive to this day?"; 
Console.WriteLine(question3);
Console.WriteLine("----");
MemoryAnswer answer3 = await memory.AskAsync(question3); 
Console.WriteLine(answer3.Result);
Console.WriteLine();
printCitation(answer3);

string question3b = "Are dinosaurs alien?";
Console.WriteLine(question3b);
Console.WriteLine("----");
MemoryAnswer answer3b = await memory.AskAsync(question3b, filter: new MemoryFilter { "reddit" });
Console.WriteLine(answer3b.Result);
Console.WriteLine();
printCitation(answer3b);

string question4 = "What is kernel memory? How do I instantiate one with KernelMemoryBuilder in .NET for Azure OpenAI? Ignore the import and config. Do not leave the code incomplete.";
Console.WriteLine(question4);
Console.WriteLine("----");
MemoryAnswer answer4 = await memory.AskAsync(question4);
Console.WriteLine($"Kernel Memory Results: \n {answer4.Result}");
Console.WriteLine();
printCitation(answer4);

agentThread.ChatHistory.AddAssistantMessage($"Results of the previous Kernel Memory results: {answer4.Result}");
var userPrompt = $"Please take this result, and fix or complete the provided code. The code should mostly be correct. Fix syntax and fill in any missing gap. The results and code is in the line below: \n {answer4.Result}";
var userInput = new ChatMessageContent(AuthorRole.User, userPrompt);
var answer4a = devAgent.InvokeAsync(userInput, agentThread);
Console.WriteLine($"Chat AI Agent Contextualized Results: \n{answer4a.ToArrayAsync().Result.Last().Message.Content}");
Console.WriteLine();


void printCitation(MemoryAnswer answer)
{
    // Citations
    Console.WriteLine("Answer Sources:");

    foreach (var source in answer.RelevantSources)
    {
        var citationUpdates = $"{source.Partitions.First().LastUpdate:D}";
        var citation = $"* {source.SourceContentType} -- (docId: {source.DocumentId})   -- {citationUpdates}";

        ansiConsole.WriteLine($"{citation}");
    }
    Console.WriteLine();
}
#pragma warning restore
