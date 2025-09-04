using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using new_sk_labs.Helper;
using new_sk_labs.Plugins;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentHelper = new_sk_labs.Helper.AgentHelper;
using new_sk_labs.Shared;


#pragma warning disable
namespace new_sk_labs.Steps
{
    public class WriteContentStep : KernelProcessStep<ContentProcessResults>
    {
        private ContentProcessResults? _state;

        public static class Functions
        {
            public const string WriteContent = nameof(WriteContentStep);
            public const string ReviseContent = nameof(ReviseContent);
        }

        public static class Events
        {
            public const string WriteContentComplete = nameof(WriteContentComplete);
            public const string WriteContentRevise = nameof(WriteContentRevise);
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.WriteContent)]
        public async ValueTask WriteContentAsync(KernelProcessStepContext context, ContentProcessResults request) 
        {
            Console.WriteLine("Step 2 - Writing content for a specific request ...\n");
            Console.WriteLine(request.StoryRequest);

            Kernel kernel = KernelHelper.BuildKernel();
            kernel.ImportPluginFromType<WriterPlugin>();

            var kernelExecutionSettings = KernelHelper.GetDefaultOpenAIPromptExecutionSettings();
            var kernelArguments = KernelHelper.BuildKernelArguments(kernelExecutionSettings);

            ChatCompletionAgent agent = AgentHelper.BuildAgent(
                kernel,
                kernelArguments,
                name: "WriterAgent",
                instruction: "You are a writer who will write informative facts about any topics of your choosing or one story specified by the user, if the subject or topic is provided. You write only factual content or story content, but not both.",
                description: "You are a writer who write informative content and/oe story.");

            var agentThead = AgentHelper.CreateChatHistoryAgentThread();
            string initPrompt = request.StoryRequest;
            var response = AgentHelper.GetAgentResponseAsync(agent, agentThead, initPrompt);

            this._state!.Content = response.Result;
            await context.EmitEventAsync(new() { Id = Events.WriteContentComplete, Data = this._state, Visibility = KernelProcessEventVisibility.Public });
        }

        [KernelFunction(Functions.ReviseContent)]
        public async ValueTask ReviseContentAsync(KernelProcessStepContext context, ContentProcessResults content)
        {
            Console.WriteLine("Step 3a - Revising content for a specific request ...\n");

            Kernel kernel = KernelHelper.BuildKernel();
            kernel.ImportPluginFromType<WriterPlugin>();

            var kernelExecutionSettings = KernelHelper.GetDefaultOpenAIPromptExecutionSettings();
            var kernelArguments = KernelHelper.BuildKernelArguments(kernelExecutionSettings);

            ChatCompletionAgent agent = AgentHelper.BuildAgent(
                kernel,
                kernelArguments,
                name: "EditorAgent",
                instruction: $"You are an editor and tasked to rewrite and a content you just wrote. \n The subject prompt was {content.StoryRequest}. \n The original content is `{content.Content}`.",
                description: "You are an enditor tasked rewrite the content just provided to you.");

            var agentThead = AgentHelper.CreateChatHistoryAgentThread();
            string initPrompt = content.Content;
            var response = AgentHelper.GetAgentResponseAsync(agent, agentThead, initPrompt);

            this._state!.Content = response.Result;
            await context.EmitEventAsync(new() { Id = Events.WriteContentRevise, Data = this._state, Visibility = KernelProcessEventVisibility.Public });
        }
    }
}
#pragma warning disable
