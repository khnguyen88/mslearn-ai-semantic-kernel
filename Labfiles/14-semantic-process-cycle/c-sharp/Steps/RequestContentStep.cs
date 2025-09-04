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

        public static class Events
        {
            public const string RequestComplete = nameof(RequestComplete);
            public const string RequestCancelled = nameof(RequestCancelled);

        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.RequestContent)]
        public async Task RequestContentAsync(KernelProcessStepContext context)
        {
            Console.WriteLine("Step 1 - Requesting content subject...\n");
            Console.WriteLine("");
            Console.WriteLine("In terms of content what would you like me to write about? A informative article about a topic of your choosing? A creative story with a particular subject and theme?? Or Would you like me to decide?");
            string input = string.Empty;
            input = Console.ReadLine();
            Console.WriteLine("");

            if (input.Contains("EXIT") || input.Contains("CANCEL")) {
                await context.EmitEventAsync(new() { Id = Events.RequestCancelled, Data = null, Visibility = KernelProcessEventVisibility.Public });
            }
            else
            {
                this._state!.StoryRequest = input;
                await context.EmitEventAsync(new() { Id = Events.RequestComplete, Data = this._state, Visibility = KernelProcessEventVisibility.Public });
            }

        }
    }
}
#pragma warning disable
