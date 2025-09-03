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