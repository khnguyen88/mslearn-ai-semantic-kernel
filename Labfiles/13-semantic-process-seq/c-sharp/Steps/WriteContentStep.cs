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
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.WriteContent)]
        public async ValueTask<ContentProcessResults> WriteContentAsync(KernelProcessStepContext context, ContentProcessResults request) 
        {
            Console.WriteLine("Step 2 - Writing content for a specific request ...\n");

            Kernel kernel = KernelHelper.BuildKernel();
            kernel.ImportPluginFromType<WriterPlugin>();

            var kernelExecutionSettings = KernelHelper.GetDefaultOpenAIPromptExecutionSettings();
            var kernelArguments = KernelHelper.BuildKernelArguments(kernelExecutionSettings);

            ChatCompletionAgent agent = AgentHelper.BuildAgent(
                kernel,
                kernelArguments,
                name: "WriterAgent",
                instruction: "You are a writer who will write informative facts about any topics of your choosing or one that is epcified by the user, if the subject or topic is provided.",
                description: "Your or a information writer about general topics");

            var agentThead = AgentHelper.CreateChatHistoryAgentThread();
            string initPrompt = request.Content;
            var response = AgentHelper.GetAgentResponseAsync(agent, agentThead, initPrompt);

            this._state!.Content = response.Result;

            //Note: EmitEventAsync() not needed unless we want to create a loopback or condiitonal process
            //await context.EmitEventAsync(new() { Id = "WriteContentComplete", Data = this._state, Visibility = KernelProcessEventVisibility.Public }); 

            return this._state;
        }
    }
}
#pragma warning disable
