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
            Console.WriteLine();
            Console.WriteLine("Step 3 - Let's validate the content before it is published...");
            Console.WriteLine("Do you like the current content? Answer with only 'YES 'or 'NO'");


            this._state!.StoryRequest = content.StoryRequest;
            this._state!.Content = content.Content;

            string input = GetValidatedInput("YES", "NO");
            bool conditionalCheck = false;

            if (input.Equals("NO", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("");
                Console.WriteLine("Do you wish to revise the current content or write a new one? Answer with only 'REVISE' or 'NEW'");
                string input2 = GetValidatedInput("REVISE", "NEW");


                if (input2.Equals("REVISE", StringComparison.OrdinalIgnoreCase))
                {
                    await context.EmitEventAsync(new() { Id = Events.ReviseContent, Data = this._state, Visibility = KernelProcessEventVisibility.Public });

                }
                else
                {
                    await context.EmitEventAsync(new() { Id = Events.RedoContent, Data = this._state, Visibility = KernelProcessEventVisibility.Public });
                }
            }
            else
            {
                await context.EmitEventAsync(new() { Id = Events.ApproveContent, Data = this._state, Visibility = KernelProcessEventVisibility.Public });
            }
        }

        private static string GetValidatedInput(string option1, string option2)
        {
            string input;
            bool isValid;
            do
            {
                input = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;
                isValid = input.Equals(option1.ToUpper()) || input.Equals(option2.ToUpper());

                if (!isValid)
                {
                    Console.WriteLine($"\nInvalid input. Please answer with only '{option1.ToUpper()}' or '{option2.ToUpper()}'.");
                }
            }
            while (!isValid);

            return input;
        }
    }
}
#pragma warning disable
