using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using new_sk_labs.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace new_sk_labs.Helper
{
    public static class KernelHelper
    {
        public static Kernel BuildKernel()
        {
            var config = new ConfigurationBuilder().AddUserSecrets("d8159b05-7d55-4374-8785-8a023929f4ce").Build();

            string deploymentName = config["AzureAIFoundry:AIModel:Name"]!;
            string endpoint = config["AzureAIFoundry:AIModel:Uri"]!;
            string apiKey = config["AzureAIFoundry:AIModel:ApiKey"]!;

            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            var kernel = kernelBuilder.Build();

            return kernel;
        }

        public static Kernel CloneKernel(Kernel kernel)
        {
            return kernel.Clone();
        }

        public static OpenAIPromptExecutionSettings GetDefaultOpenAIPromptExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
        }

        public static KernelArguments BuildKernelArguments(OpenAIPromptExecutionSettings openAIPromptExecutionSettings)
        {
            return new KernelArguments(openAIPromptExecutionSettings);
        }
    }
}