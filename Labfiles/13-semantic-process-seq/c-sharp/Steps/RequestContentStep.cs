using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using new_sk_labs.Helper;
using new_sk_labs.Plugins;
using new_sk_labs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentHelper = new_sk_labs.Helper.AgentHelper;


#pragma warning disable
namespace new_sk_labs.Steps
{
    public class RequestContentStep : KernelProcessStep<ContentProcessResults>
    {
        private ContentProcessResults? _state;

        public static class Functions
        {
            public const string RequestContent = nameof(RequestContentStep);
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.RequestContent)]
        public async ValueTask<ContentProcessResults> RequestContentAsync(KernelProcessStepContext context)
        {
            Console.WriteLine("Step 1 - Requesting content subject...\n");
            Console.WriteLine("");
            Console.WriteLine("In terms of content what would you like me to write about? A informative article about a topic of your choosing? A creative story with a particular subject and theme?? Or Would you like me to decide?");
            string input = string.Empty;
            input = Console.ReadLine();
            Console.WriteLine("");

            this._state!.Content = input;

            //Note: EmitEventAsync() not needed unless we want to create a loopback or condiitonal process
            //await context.EmitEventAsync(new() { Id = "RequestContentComplete", Data = this._state, Visibility = KernelProcessEventVisibility.Public });

            return this._state;
        }
    }

    public class RequestContentState
    {
        public string LastGeneratedDocument { get; set; } = "";
        public ChatHistory? ChatHistory { get; set; }
    }
}
#pragma warning disable
