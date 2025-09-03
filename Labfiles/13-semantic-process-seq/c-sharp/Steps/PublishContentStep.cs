using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using new_sk_labs.Helper;
using new_sk_labs.Plugins;
using new_sk_labs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#pragma warning disable
namespace new_sk_labs.Steps
{
    public sealed class PublishContentStep : KernelProcessStep<ContentProcessResults>
    {
        private ContentProcessResults? _state;

        public static class Functions
        {
            public const string PublishContent = nameof(EditContentStep);
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.PublishContent)]
        public async ValueTask<ContentProcessResults> PublisContentAsync(KernelProcessStepContext context, ContentProcessResults content)
        {
            Console.WriteLine("Step 3 - Doing Yet More Work...\n");

            Kernel kernel = KernelHelper.BuildKernel();

            var kernelExecutionSettings = KernelHelper.GetDefaultOpenAIPromptExecutionSettings();
            var kernelArguments = KernelHelper.BuildKernelArguments(kernelExecutionSettings);

            ChatCompletionAgent agent = AgentHelper.BuildAgent(
                kernel,
                kernelArguments,
                name: "PublisherAgent",
                instruction: $"You are a publisher. Please take the content provided by the user and format it into markdown format. Please make it into a blog format, with a catchy title and headliner, a date from {DateTime.Now}, and the body content reqrited and structured to be more reader friendly. Bulletins are acceptable.",
                description: "You are a copy editor and publisher who improve the writer content for a blog");

            var agentThead = AgentHelper.CreateChatHistoryAgentThread();
            string initPrompt = $"Here is the content from the writer: \n {content}";
            var response = AgentHelper.GetAgentResponseAsync(agent, agentThead, initPrompt);

            this._state!.Content = response.Result;
            await context.EmitEventAsync(new() { Id = "WriteContentComplete", Data = this._state, Visibility = KernelProcessEventVisibility.Public });
            return this._state;
        }
    }
}
#pragma warning disable
