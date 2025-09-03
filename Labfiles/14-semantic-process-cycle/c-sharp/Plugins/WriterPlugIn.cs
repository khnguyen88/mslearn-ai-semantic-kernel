using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace new_sk_labs.Plugins
{
    class WriterPlugin
    {
        // Create a kernel function to build the stage environment
        [KernelFunction("CreativeStoryNoPrompt")]
        [Description("Creates a creative story if the user does not specify a specific prompt. The creative story can based on whatever subject and category the writer agent desire. However the story must be under 150 words. Include the method return value before the generated content.")] // Optional
        [return: Description("Returns a creative story based on whatever subject and category the writer agent desire. However the story must be under 150 words. ")] //Optional
        string CreativeStoryAny()
        {
            return "Here is a randome creative story: \n";
        }

        [KernelFunction("CreativeStoryWithPrompts")]
        [Description("Creates a creative story based on user the user prompts.The creative story must fall under the category the user desire and must be about the subjected they requested.However the story must be under 150 words. Include the method return value before the generated content.")] // Optional
        [return: Description("Returns a creative story based on user the user prompts. The creative story must fall under the category the user desire and must be about the subjected they requested. However the story must be under 150 words.")] //Optional
        string CreativeStoryAny(string storyCategory, string subjectPrompt)
        {
            return $"Here is a creative story based on your requested {storyCategory} focusing on this subject prompt, {subjectPrompt}: \n";
        }

        [KernelFunction("WrittenInformationFacts")]
        [Description("Provide somne information or facts in paragraph format about a subject specified by the user. The facts must be correct and must not be false. The written information bust be under 150 words or less. Include the method return value before the generated content.")] // Optional
        [return: Description("The explicit response an assistant should provide back to the user with no additional add on after the assistant has process their request.")] //Optional
        string WrittenFacts(string subject)
        {
            return $"Here is some information or facts based on your subject or topic, {subject}: \n";
        }
    }
}
