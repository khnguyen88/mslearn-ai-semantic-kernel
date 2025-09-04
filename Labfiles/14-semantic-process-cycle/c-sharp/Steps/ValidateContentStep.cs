using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using new_sk_labs.Shared;


#pragma warning disable
namespace new_sk_labs.Steps
{
    public sealed class ValidateContentStep : KernelProcessStep<ContentProcessResults>
    {
        private ContentProcessResults? _state;

        public static class Functions
        {
            public const string ValidateContent = nameof(ValidateContentStep);
        }

        public static class Events
        {
            public const string ReviseContent = nameof(ReviseContent);
            public const string RedoContent = nameof(RedoContent);
            public const string ApproveContent = nameof(ApproveContent);
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ContentProcessResults> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        [KernelFunction(Functions.ValidateContent)]
        public async ValueTask ValidateContentAsync(KernelProcessStepContext context, ContentProcessResults content)
        {
            Console.WriteLine("Step 3 - Let's validate the content before it is published...\n");
            Console.WriteLine("");
            Console.WriteLine("Do you like the current content? Answer with only 'YES 'or 'NO'");


            this._state = content;

            string input = string.Empty;
            bool conditionalCheck = false;

            do {
                input = Console.ReadLine();
                Console.WriteLine("");

                conditionalCheck = input.Contains("YES") || input.Contains("NO");
            }
            while (!conditionalCheck);

            if (input.Contains("NO"))
            {
                Console.WriteLine("");
                Console.WriteLine("Do you wish to revise the current content or write a new one? Answer with only 'REVISE' or 'NEW'");
                string input2 = string.Empty;
                bool conditionalCheck2 = false;

                do
                {
                    input = Console.ReadLine();
                    Console.WriteLine("");

                    conditionalCheck = input.Contains("REVISE") || input.Contains("NEW");
                }
                while (!conditionalCheck);

                if (input.Contains("REVISE"))
                {
                    await context.EmitEventAsync(new() { Id = Events.ReviseContent, Data = content, Visibility = KernelProcessEventVisibility.Public });
                }
                else
                {
                    await context.EmitEventAsync(new() { Id = Events.RedoContent, Data = null, Visibility = KernelProcessEventVisibility.Public });
                }
            }
            else
            {
                await context.EmitEventAsync(new() { Id = Events.ApproveContent, Data = content, Visibility = KernelProcessEventVisibility.Public });
            }
        }
    }
}
#pragma warning disable
