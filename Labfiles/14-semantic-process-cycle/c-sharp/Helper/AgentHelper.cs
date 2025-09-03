using Azure;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace new_sk_labs.Helper
{
    public static class AgentHelper
    {
        public static ChatCompletionAgent BuildAgent(Kernel kernel, KernelArguments kernelArguments, string name, string instruction, string description)
        {
            return new ChatCompletionAgent() {
                Name = name,
                Instructions = instruction,
                Description = description,
                Kernel = kernel,
                Arguments = kernelArguments
            };
        }

        public static ChatHistoryAgentThread CreateChatHistoryAgentThread() {
            return new ChatHistoryAgentThread();
        }

        public static void AddMessageToChatHistoryThread(ChatHistoryAgentThread agentThread, AuthorRole role, string messageContent)
        {
            if (role == AuthorRole.System)
            {
                agentThread.ChatHistory.AddSystemMessage(messageContent);
            }
            else if (role == AuthorRole.Assistant)
            {
                agentThread.ChatHistory.AddAssistantMessage(messageContent);
            }
            else { 
                agentThread.ChatHistory.AddUserMessage(messageContent);
            }
        }

        public static void AddMessageToChatHistoryThread(ChatHistoryAgentThread agentThread, ChatMessageContent chatMessageContent)
        {
            agentThread.ChatHistory.Add(chatMessageContent);
        }

        public static ChatMessageContent CreateChatMessageContent(AuthorRole authorRole, string messageContent)
        {
            return new ChatMessageContent(authorRole, messageContent);
        }


        public static async Task<string> GetAgentResponseAsync(ChatCompletionAgent agent, ChatHistoryAgentThread agentThread, string uswrMessage)
        {
            string? fullResponse = string.Empty;
            
            Console.WriteLine("");
            await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(uswrMessage, agentThread))
            {
                Console.Write($"{response.Content}"); // Stream to console
                fullResponse = fullResponse + (response.Content ?? "");
            }
            Console.WriteLine("");
            return fullResponse;
        }
    }
}
