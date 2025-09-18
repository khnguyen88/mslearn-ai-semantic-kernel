
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
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
// dotnet add package Microsoft.KernelMemory.DocumentStorage.AzureBlobs

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

string blobStorageAccount = config["AzureAIFoundry:AzureBlobStorage:AccountName"]!;
string blobStorageContainerName = config["AzureAIFoundry:AzureBlobStorage:ContainerName"]!;
string blobStorageEndpoint = config["AzureAIFoundry:AzureBlobStorage:Uri"]!;
string blobStorageKey = config["AzureAIFoundry:AzureBlobStorage:ApiKey"]!;


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

AzureBlobsConfig azureBlobsConfig = new()
{
    Account = blobStorageAccount,
    AccountKey = blobStorageKey,
    Container = blobStorageContainerName,
    Auth = AzureBlobsConfig.AuthTypes.AccountKey,
};


IKernelMemoryBuilder memoryBuilder = new KernelMemoryBuilder()
    //.WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk }) // Allows local persistent storage (in the bin folder). Only do one type.
    //.WithSimpleVectorDb(new SimpleVectorDbConfig {StorageType = FileSystemTypes.Disk }) // Allow local persistent storage (in the bin folder). Only do one type.
    .WithAzureOpenAITextGeneration(textAzureOpenAIConfig)
    .WithAzureOpenAITextEmbeddingGeneration(embeddingAzureOpenAIConfig)
    .WithAzureAISearchMemoryDb(azureAISearchConfig)
    .WithAzureBlobsDocumentStorage(azureBlobsConfig);

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
//Based on persistent and cloud version, it appears running the memory will just override the document given the documentId.


var docId = await memory.ImportTextAsync(
    text: "Khiem had a cat name Benzi. She was a calico", 
    documentId: "khiem-benzi", 
    tags: new TagCollection() {
        { "author", new List<string?>{"khiem"}},
        { "source", new List<string?>{"khiem"}},
        { "credible", new List<string?>{"yes"}},
        { "topic", new List<string?>{"human-pet-relationship", "pet"}},
        { "person", new List<string>{"khiem"}},
        { "pet", new List<string>{"benzi"}}
    }
);

//Console.WriteLine(JsonSerializer.Serialize(docId)); // Output the a generated documentId, if one has not been specified

await memory.ImportTextAsync(
    "Tom The Terrible Train was not a train but a tank.", 
    documentId: "tom-tank"
);

await memory.ImportDocumentAsync(
    "./docs/facts.txt", 
    "matt-eland-repository-facts"
);

var docId2 = await memory.ImportWebPageAsync(
    url: "https://en.wikipedia.org/wiki/Dinosaur", 
    documentId: "wiki-dinosaur",
    tags: new TagCollection() {
        { "author", new List<string?>{"wikipedia", "wiki"}},
        { "source", new List<string?>{"wikipedia", "wiki"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"dinosaur"}},
    }
);
//Console.WriteLine(JsonSerializer.Serialize(docId2)); // Output the documentId specified

await memory.ImportDocumentAsync(
    "./docs/reddit-dinosaurs.txt", 
    documentId: "reddit-dinosaur",
    tags: new TagCollection() {
        { "author", new List<string?>{"reddit, reddit-users"}},
        { "source", new List<string?>{"reddit"}},
        { "credibility", new List<string?>{"none", "low"}},
        { "topic", new List<string?>{"dinosaur"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://github.com/microsoft/kernel-memory/tree/main", 
    documentId: "microsoft-github-kernelmemory-page",
    tags: new TagCollection() {
        { "author", new List<string?>{"microsoft"}},
        { "source", new List<string?>{"github"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"kernel-memory"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://github.com/IntegerMan/DocumentSearchWithKernelMemory/blob/main/MattEland.Demos.KernelMemory.DocumentSearch/MattEland.Demos.KernelMemory.DocumentSearch/Program.cs", 
    documentId: "matt-eland-kernel-memory-search-code",
    tags: new TagCollection() {
        { "author", new List<string?>{"matt-eland"}},
        { "source", new List<string?>{"github"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"kernel-memory"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://blog.leadingedje.com/post/ai/documents/kernelmemory.html", 
    documentId: "leadingEDJE-kernel-memory-search-blog",
    tags: new TagCollection() {
        { "author", new List<string?>{"matt-eland", "leadingEDJE", "leading-EDJE", "leading-edje"}},
        { "source", new List<string?>{"leadingEDJE", "leading-EDJE", "leading-edje"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"kernel-memory"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://johnnyreilly.com/using-kernel-memory-to-chunk-documents-into-azure-ai-search",
    documentId: "johnny-reilly-kernel-memory-ai-search",
    tags: new TagCollection() {
        { "author", new List<string?>{"john-reilly"}},
        { "source", new List<string?>{"john-reilly", "john-reilly-blog"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"kernel-memory", "ai-search", "kernel-memory-ai-search"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://dev.to/stormhub/kernel-memory-with-azure-openai-blob-storage-and-ai-search-services-1245", 
    documentId: "dev-community-kernel-memory-ai-search-blob-storage",
    tags: new TagCollection() {
        { "author", new List<string?>{"dev-community", "storm-hub", "dev.io"}},
        { "source", new List<string?>{"dev-community", "storm-hub", "dev.io", "dev-community-blog"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{ "kernel-memory", "ai-search", "azure-blob-storage", "kernel-memory-ai-search-blob-storage", "kernel-memory-ai-search"}},
    }
);

await memory.ImportWebPageAsync(
    url: "https://github.com/StormHub/stormhub/blob/main/resources/2024-11-18/ConsoleApp/ConsoleApp/Program.cs", 
    documentId: "storm-hub-kernel-memory-ai-search-blob-storage",
    tags: new TagCollection() {
        { "author", new List<string?>{"dev-community", "storm-hub", "dev.io"}},
        { "source", new List<string?>{"dev-community", "github", "storm-hub"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{ "kernel-memory", "ai-search", "azure-blob-storage", "kernel-memory-ai-search-blob-storage", "kernel-memory-ai-search"}},
    }
);
await memory.ImportWebPageAsync(
    url: "https://github.com/microsoft/kernel-memory/issues/203", 
    documentId: "microsoft-kernel-memory-issue-persistent-memory",
    tags: new TagCollection() {
        { "author", new List<string?>{"matt-eland"}},
        { "source", new List<string?>{"github"}},
        { "credibility", new List<string?>{"high"}},
        { "topic", new List<string?>{"kernel-memory", "ai-search", "in-memory-storage", "local-storage", "cloud-storage", "azure-blob-storage", "kernel-memory-ai-search-blob-storage"}},
    }
);

//await memory.DeleteDocumentAsync("reddit-dinosaur"); //Allows you to delete data based on document Id


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
PrintKernelMemoryCitation(answer);

string question2 = "Who and what is Tom?";
Console.WriteLine(question2);
Console.WriteLine("----");
MemoryAnswer answer2 = await memory.AskAsync(question2);
Console.WriteLine(answer2.Result);
Console.WriteLine();
PrintKernelMemoryCitation(answer2);

string question3 = "Did dinosaurs survive to this day?"; 
Console.WriteLine(question3);
Console.WriteLine("----");
MemoryAnswer answer3 = await memory.AskAsync(question3); 
Console.WriteLine(answer3.Result);
Console.WriteLine();
PrintKernelMemoryCitation(answer3);

Console.WriteLine("With Tags");
string question3b = "Are dinosaurs alien?";
Console.WriteLine(question3b);
Console.WriteLine("----");
MemoryAnswer answer3b = await memory.AskAsync(question3b, filter: new MemoryFilter().ByTag("source", "reddit")); //Filters out other sources that doesnt contain the specified tag(s)
Console.WriteLine(answer3b.Result);
Console.WriteLine();
PrintKernelMemoryCitation(answer3b);


Console.WriteLine("Without Tags");
string question3c = "Are dinosaurs alien?";
Console.WriteLine(question3c);
Console.WriteLine("----");
MemoryAnswer answer3c = await memory.AskAsync(question3c, filter: new MemoryFilter());
Console.WriteLine(answer3c.Result);
Console.WriteLine();
PrintKernelMemoryCitation(answer3c);

string question4 = "What is kernel memory? How do I instantiate one with KernelMemoryBuilder in .NET for Azure OpenAI? Ignore the import and config. Do not leave the code incomplete.";
Console.WriteLine(question4);
Console.WriteLine("----");
MemoryAnswer answer4 = await memory.AskAsync(question4);
Console.WriteLine($"Kernel Memory Results: \n {answer4.Result}");
Console.WriteLine();
PrintKernelMemoryCitation(answer4);

string question5 = "In Kernel Memory what line of code do I need to ask async or query, basically perform rag get the results from the memory?";
Console.WriteLine(question5);
Console.WriteLine("----");
MemoryAnswer answer5 = await memory.AskAsync(question5);
Console.WriteLine($"Kernel Memory Results: \n {answer5.Result}");
Console.WriteLine();
PrintKernelMemoryCitation(answer5);


// Utilizing Semantic Kernel and An AI Agent or Chat Completion to contextualize the last two question
// =====================================================================================
//Contextualize kernel memory results with feedback from text generation LLM
//An Aside Thought:
//  It appears the kernel memory response is based on how it partitions and embeds the contents in a document, some results may be incomplete as with the code questions.
//  I think we can use AI Agent orchestration to essentially contextualize, fix, and complete segmented RAG responses.
//  A process and a concurrent or even magentic orchestration would be very useful in this context
//  The key is to ask very specific questions about components of the code, and take the results and combine them.

agentThread.ChatHistory.AddAssistantMessage($"Results of the previous Kernel Memory questions:\n{answer4.Result} \n and \n {answer5.Result}");
var userPrompt = $"Please take this result, and fix or complete the provided code. The code should mostly be correct. Fix syntax and fill in any missing gap. Combine the results if needed into one cohesive solution. The results and code is in the line below: \n {answer4.Result} and \n{answer5.Result}";
var userInput = new ChatMessageContent(AuthorRole.User, userPrompt);
var answer4a = devAgent.InvokeAsync(userInput, agentThread);
Console.WriteLine($"Chat AI Agent Contextualized Results: \n{answer4a.ToArrayAsync().Result.Last().Message.Content}");
Console.WriteLine();

void PrintKernelMemoryCitation(MemoryAnswer answer)
{
    // Citations
    Console.WriteLine("Answer Sources:");

    foreach (var source in answer.RelevantSources)
    {
        var citationUpdates = $"{source.Partitions.First().LastUpdate:D}";
        var citation = $"* {source.SourceContentType} -- (docId: {source.DocumentId})   -- {citationUpdates}";

        Console.WriteLine(source.SourceUrl != null
            ? $"  - {source.SourceUrl} [{source.Partitions.First().LastUpdate:D}]"
            : $"  - {source.SourceName}  - {source.Link} [{citationUpdates}]");
    }
    Console.WriteLine();
}
#pragma warning restore

//TODO: Explore how we can control document clean-up, partitioning, and embedding. Creating our own custom pipeline for this process but give us better control of our results
