using MCP.Host.Agents.CodingAgent.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace MCP.Host.Agents.CodingAgent.Steps;

public class AgentGroupChatStep : KernelProcessStep
{
    public static class ProcessStepFunctions
    {
        public const string INVOKE_AGENT_GROUP = nameof(INVOKE_AGENT_GROUP);
    }

    [KernelFunction(ProcessStepFunctions.INVOKE_AGENT_GROUP)]
    public async Task InvokeAgentGroupAsync(KernelProcessStepContext context, Kernel kernel, string input)
    {
        var logger = kernel.GetRequiredService<ILogger<InputCheckStep>>();
        var chat = kernel.GetRequiredService<AgentGroupChat>();

        chat.IsComplete = false;

        ChatMessageContent message = new(AuthorRole.User, input);
        chat.AddChatMessage(message);
        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = AgentOrchestrationEvents.GroupMessage,
            Data = message
        });

        await InvokeGroupChatAsync(context, chat, logger);
        
        var history = await GetChatHistoryAsync(chat);

        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = AgentOrchestrationEvents.GroupCompleted, 
            Data = history
        });
    }

    private async Task InvokeGroupChatAsync(KernelProcessStepContext context, AgentGroupChat chat, ILogger<InputCheckStep> logger)
    {
        var isResponseEmpty = false;
        await foreach (var response in chat.InvokeAsync())
        {
            logger.LogInformation("Agent group chat response from {author}: {response}", response.AuthorName, response.Content);
            if (string.IsNullOrWhiteSpace(response.Content))
            {
                isResponseEmpty = true;
                break;
            }
            await context.EmitEventAsync(new KernelProcessEvent
            {
                Id = AgentOrchestrationEvents.GroupMessage,
                Data = response
            });
        }

        if (isResponseEmpty)
        {
            ChatMessageContent message = new(AuthorRole.Developer, "Please continue your work! Use the tools whenever possible!");
            chat.AddChatMessage(message);
            await InvokeGroupChatAsync(context, chat, logger);
        }
    }

    private static async Task<string> GetChatHistoryAsync(AgentGroupChat chat)
    {
        var sb = new StringBuilder();
        await foreach (var message in chat.GetChatMessagesAsync())
        {
            sb.AppendLine(message.ToString());
        }
        return sb.ToString();
    }
}