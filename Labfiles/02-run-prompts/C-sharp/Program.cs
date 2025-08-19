using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Dynamic;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Obtain your API access information
// ---------------------------------------------------------------
//string filePath = Path.GetFullPath("appsettings.json");
//var config = new ConfigurationBuilder()
//    .AddJsonFile(filePath)
//    .Build();

var config = new ConfigurationBuilder().AddUserSecrets("968d9b73-5aa8-4f11-b52d-51e77bd74408").Build();


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


// Create the chat history
// ---------------------------------------------------------------
var chatHistory = new ChatHistory();

    //Misc (Not part of tutorial, but for record keeping)
    // Other ways to declare, initialize, and populate the ChatHistory

    // Alt, Ver  1
    var chatHistory2 = new ChatHistory(); // Initialize Chat History with no message
    chatHistory.AddSystemMessage("You speak in proper british english"); // Add Message, Option #1 (Specific methods [AddSystemMessage, AddUserMessage, AddAssistantMessage]
    chatHistory.AddMessage(AuthorRole.User, "What is britian's most popular tea?"); // Add Message, Option #2 (General, Specific Role via parameters)

    // Alt, Ver 2
    var chatHistory3 = new ChatHistory() {
        new ChatMessageContent( AuthorRole.System, "Speak in proper british english"),
        new ChatMessageContent( AuthorRole.User, "What is britian's most popular tea?")
};

// Create a semantic kernel prompt template
// ---------------------------------------------------------------
// Note: It was not included in the tutorial, but we need to define the prompt string with Semantic Kernel's native template syntax
// Note2:
    //  Variables: Inserting values from the context, e.g., {{$name}}.
    //  Function Calls: Calling other functions (like a GetWeather function) to insert dynamic content, e.g., {{weather.GetForecast}}.

var skTemplateFactory = new KernelPromptTemplateFactory();

// Note 3: PromptTemplateConfig provides the configuration information necessary to create a prompt template.
var promptTemplateConfig = new PromptTemplateConfig(
    """
    You are a helpful career advisor. Based on the users's skills and interest, suggest up to 5 suitable roles.
    Return the output as JSON in the following format:
    "Role Recommendations":
    {
    "recommendedRoles": [],
    "industries": [],
    "estimatedSalaryRange": ""
    }

    My skills are: {{$skills}}. My interests are: {{$interests}}. My goals are: {{$goals}}. What are some roles that would be suitable for me?
    """
);

var skPromptTemplate = skTemplateFactory.Create(promptTemplateConfig);


// Render the Semantic Kernel prompt with arguments
// ---------------------------------------------------------------
var skPromptArguments = new KernelArguments {
    ["skills"] = "Software Engineering, C#, Python, Drawing, Guitar, Dance",
    ["interests"] = "Education, Psychology, Programming, Helping Others",
    ["goals"] = "Help people and animals, Make a livable wage with enough to contribute to retirement fund"
};
var skRenderedPrompt = await skPromptTemplate.RenderAsync(kernel, skPromptArguments);

// Add the Semanitc Kernel prompt to the chat history and get the reply
// ---------------------------------------------------------------
Console.WriteLine(skRenderedPrompt);
chatHistory.AddUserMessage(skRenderedPrompt);
await GetReply();

// Create a handlebars template
// ---------------------------------------------------------------
// Note 1: Handlebars template allows you to add conditional logic and loops, making it much more dynamic.
    // Note 1A: https://learn.microsoft.com/en-us/semantic-kernel/concepts/prompts/handlebars-prompt-templates?pivots=programming-language-csharp
    // Note 1B: Use $ when referring to root-level variables 
    // Note 1C: Omit $ when properties of an object or items within a collection
    // Note 1D: https://handlebarsjs.com/guide/
// Note 2: In this example we are using special XML-like tags to specify roles of each prompts

var hbTemplateFactory = new HandlebarsPromptTemplateFactory();

var promptTemplateConfig2 = new PromptTemplateConfig
{
    TemplateFormat = "handlebars",
    Name = "MissingSkillsPrompt",
    Template = """
        <message role="system">
        Instructions: You are a career advisor. Analyze the skill gap between 
        the user's current skills and the requirements of the target role.
        </message>

        <message role="user">
        Target Role: {{targetRole}}
        </message>

        <message role="user">
        Current Skills:  {{currentSkills}}
        </message>

        <message role="user">
        {{#if futureSkills}}
        I've recently acquired some new skills.

        {{#each futureSkills}}
        In the {{FieldName}} field, I am current working towards learning the {{Skill}}.
        {{/each}}

        {{else}}
        I am acquiring no other future skills other than the one specified.

        {{/if}}
        <message role="assistant">
        "Skill Gap Analysis":
        {
            "missingSkills": [],
            "coursesToTake": [],
            "certificationSuggestions": []
        }
        </message>
    """
};

var hbPromptTemplate = hbTemplateFactory.Create(promptTemplateConfig2);


// Render the Handlebars prompt with arguments
// ---------------------------------------------------------------

var futureSkills = new List<FieldSkill>() {
    new FieldSkill{ FieldName = "Computer Science", Skill = "AI and AI Orchestration" },
    new FieldSkill{ FieldName = "Math", Skill = "Matrices"},
    new FieldSkill{ FieldName = "Art", Skill = "OilPainting"}
};

var hbPromptArguments = new KernelArguments
{
    ["targetRole"] = "Game Developer",
    ["currentSkills"] = "Software Engineering, C#, Python, Drawing, Guitar, Dance",
    ["futureSkills"] = futureSkills
};

var hbRenderedPrompt = await hbPromptTemplate.RenderAsync(kernel, hbPromptArguments);


// Add the Handlebars prompt to the chat history and get the reply
// ---------------------------------------------------------------
Console.WriteLine(hbRenderedPrompt);
chatHistory.AddUserMessage(hbRenderedPrompt);
await GetReply();

// Get a follow-up prompt from the user
Console.WriteLine("Assistant: How can I help you?");
Console.Write("User: ");
string userInput = Console.ReadLine()!;

// Add the user input to the chat history and get the reply
chatHistory.AddUserMessage(userInput);
await GetReply();

async Task GetReply() {
    // Get the reply from the chat completion service
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    Console.WriteLine("===================================================================================");
    Console.WriteLine("");
    chatHistory.AddAssistantMessage(reply.ToString());
}

string personName()
{
    return "Tom";
}


public class FieldSkill
{
    public string FieldName { get; set; }
    public string Skill { get; set; }
}