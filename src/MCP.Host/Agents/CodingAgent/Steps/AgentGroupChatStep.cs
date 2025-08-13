using MCP.Host.Agents.CodingAgent.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Agents.CodingAgent.Steps;

public class AgentGroupChatStep : KernelProcessStep
{
    public const string ChatServiceKey = $"{nameof(AgentGroupChatStep)}:{nameof(ChatServiceKey)}";
    public const string ReducerServiceKey = $"{nameof(AgentGroupChatStep)}:{nameof(ReducerServiceKey)}";

    public static class ProcessStepFunctions
    {
        public const string InvokeAgentGroup = nameof(InvokeAgentGroup);
    }

    [KernelFunction(ProcessStepFunctions.InvokeAgentGroup)]
    public async Task InvokeAgentGroupAsync(KernelProcessStepContext context, Kernel kernel, string input)
    {
        var chat = kernel.GetRequiredService<AgentGroupChat>();

        // Reset chat state from previous invocation
        //await chat.ResetAsync();
        chat.IsComplete = false;

        ChatMessageContent message = new(AuthorRole.User, input);
        chat.AddChatMessage(message);
        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupMessage, Data = message });

        await foreach (var response in chat.InvokeAsync())
        {
            await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupMessage, Data = response });
        }

        var history = await chat.GetChatMessagesAsync().Reverse().ToArrayAsync();

        // Summarize the group chat as a response to the primary agent
        string summary = await SummarizeHistoryAsync(kernel, ReducerServiceKey, history);

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupCompleted, Data = summary });
    }

    private static async Task<string> SummarizeHistoryAsync(Kernel kernel, string key, IReadOnlyList<ChatMessageContent> history)
    {
        var reducer = kernel.Services.GetRequiredKeyedService<ChatHistorySummarizationReducer>(key);
        var reducedResponse = await reducer.ReduceAsync(history);
        var summary = reducedResponse?.First() ?? throw new InvalidDataException("No summary available");
        return summary.ToString();
    }
}