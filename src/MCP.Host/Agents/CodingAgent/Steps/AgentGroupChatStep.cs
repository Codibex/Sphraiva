using System.Text;
using MCP.Host.Agents.CodingAgent.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

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
        var chat = kernel.GetRequiredService<AgentGroupChat>();

        chat.IsComplete = false;

        ChatMessageContent message = new(AuthorRole.User, input);
        chat.AddChatMessage(message);
        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = AgentOrchestrationEvents.GroupMessage,
            Data = message
        });

        await foreach (var response in chat.InvokeAsync())
        {
            await context.EmitEventAsync(new KernelProcessEvent
            {
                Id = AgentOrchestrationEvents.GroupMessage, 
                Data = response
            });
        }

        var history = await GetChatHistoryAsync(chat);

        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = AgentOrchestrationEvents.GroupCompleted, 
            Data = history
        });
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