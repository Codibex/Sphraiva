using System.ComponentModel;
using MCP.Host.Agents.CodingAgent.Events;
using MCP.Host.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MCP.Host.Agents.CodingAgent.Steps;

/// <summary>
/// Primary agent. This agent is responsible for managing the flow of the coding process.
/// </summary>
public class ManagerAgentStep : KernelProcessStep
{
    public const string AgentServiceKey = $"{nameof(ManagerAgentStep)}:{nameof(AgentServiceKey)}";
    public const string ReducerServiceKey = $"{nameof(ManagerAgentStep)}:{nameof(ReducerServiceKey)}";

    public static class ProcessStepFunctions
    {
        public const string InvokeAgent = nameof(InvokeAgent);
        public const string InvokeGroup = nameof(InvokeGroup);
        public const string ReceiveResponse = nameof(ReceiveResponse);
    }

    [KernelFunction(ProcessStepFunctions.InvokeAgent)]
    public async Task InvokeAgentAsync(KernelProcessStepContext context, Kernel kernel, string userInput, ILogger logger)
    {
        // Get the chat history
        var historyProvider = GetHistory(kernel);
        var history = historyProvider.Get();
        ChatHistoryAgentThread agentThread = new(history);

        // Obtain the agent response
        //ChatCompletionAgent agent = GetAgent<ChatCompletionAgent>(kernel, AgentServiceKey);
        //await foreach (ChatMessageContent message in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userInput), agentThread))
        //{
        //    // Both the input message and response message will automatically be added to the thread, which will update the internal chat history.

        //    // Emit event for each agent response
        //    await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message });
        //}

        history.AddUserMessage(userInput);

        // Evaluate current intent
        var intent = await IsRequestingUserInputAsync(kernel, history, logger);

        string intentEventId =
            intent.IsRequestingUserInput ?
                AgentOrchestrationEvents.AgentResponded :
                intent.IsWorking ?
                    AgentOrchestrationEvents.AgentWorking :
                    CommonEvents.UserInputComplete;

        await context.EmitEventAsync(new() { Id = intentEventId });
    }

    [KernelFunction(ProcessStepFunctions.InvokeGroup)]
    public async Task InvokeGroupAsync(KernelProcessStepContext context, Kernel kernel)
    {
        // Get the chat history
        var historyProvider = GetHistory(kernel);
        var history = historyProvider.Get();

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.GroupInput, Data = history.First() });
    }

    [KernelFunction(ProcessStepFunctions.ReceiveResponse)]
    public async Task ReceiveResponseAsync(KernelProcessStepContext context, Kernel kernel, string response)
    {
        // Get the chat history
        var historyProvider = GetHistory(kernel);
        var history = historyProvider.Get();

        // Proxy the inner response
        var agent = GetAgent<ChatCompletionAgent>(kernel, AgentServiceKey);
        ChatMessageContent message = new(AuthorRole.Assistant, response) { AuthorName = agent.Name };
        history.Add(message);

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponse, Data = message });

        await context.EmitEventAsync(new() { Id = AgentOrchestrationEvents.AgentResponded });
    }

    private static async Task<IntentResult> IsRequestingUserInputAsync(Kernel kernel, ChatHistory history, ILogger logger)
    {
        await Task.CompletedTask;
        return new IntentResult(false, true, string.Empty);
        //ChatHistory localHistory =
        //[
        //    new ChatMessageContent(AuthorRole.System, "Analyze the conversation and determine if user input is being solicited. Please respond with a JSON object containing only the following fields: IsRequestingUserInput, IsWorking and Rationale. Fill out the properties in all situations."),
        //    .. history.TakeLast(1)
        //];

        //IChatCompletionService service = kernel.GetRequiredService<IChatCompletionService>();

        //ChatMessageContent response = await service.GetChatMessageContentAsync(localHistory);
        //var rawText = response.ToString();
        //if (string.IsNullOrWhiteSpace(rawText))
        //{
        //    logger.LogError("Response is not valid");
        //    return new IntentResult(false, true, string.Empty);
        //}

        //try
        //{
        //    IntentResult intent = JsonSerializer.Deserialize<IntentResult>(response.ToString())!;
        //    logger.LogTrace("{StepName} Response Intent - {IsRequestingUserInput}: {Rationale}", nameof(ManagerAgentStep), intent.IsRequestingUserInput, intent.Rationale);
        //    return intent;

        //}
        //catch
        //{
        //    logger.LogError("Response is not valid: {rawText}", rawText);
        //    return new IntentResult(false, true, string.Empty);
        //}
    }

    [DisplayName("IntentResult")]
    [Description("this is the result description")]
    public sealed record IntentResult(
        [property:Description("True if user input is requested or solicited.  Addressing the user with no specific request is False.  Asking a question to the user is True.")]
        bool IsRequestingUserInput,
        [property:Description("True if the user request is being worked on.")]
        bool IsWorking,
        [property:Description("Rationale for the value assigned to IsRequestingUserInput")]
        string Rationale);

    private static IChatHistoryProvider GetHistory(Kernel kernel) =>
        kernel.Services.GetRequiredService<IChatHistoryProvider>();

    private static TAgent GetAgent<TAgent>(Kernel kernel, string key) where TAgent : Agent =>
        kernel.Services.GetRequiredKeyedService<TAgent>(key);

    private static async Task<string> SummarizeHistoryAsync(Kernel kernel, string key, IReadOnlyList<ChatMessageContent> history)
    {
        var reducer = kernel.Services.GetRequiredKeyedService<ChatHistorySummarizationReducer>(key);
        var reducedResponse = await reducer.ReduceAsync(history);
        var summary = reducedResponse?.First() ?? throw new InvalidDataException("No summary available");
        return summary.ToString();
    }
}