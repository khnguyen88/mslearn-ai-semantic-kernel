using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;


#pragma warning disable
namespace new_sk_labs.Steps
{
    public class EditContentStep : KernelProcessStep
    {
        public static class Functions
        {
            public const string EditContent = nameof(EditContentStep);
        }

        [KernelFunction(Functions.EditContent)]
        public async ValueTask ExecuteAsync(KernelProcessStepContext context)
        {
            Console.WriteLine("Step 3 - Doing Yet More Work...\n");
        }
    }
}
#pragma warning disable
