using System.Collections.Concurrent;
using Microsoft.SemanticKernel.Agents;

namespace MCP.Host.Chat;

/// <summary>
/// Caches ChatHistoryAgentThreads per ChatId and tracks last usage for cleanup.
/// </summary>
public class ChatCache
{
    private class ChatEntry
    {
        public ChatHistoryAgentThread Thread { get; }
        public DateTime LastUsedUtc { get; private set; }
        public ChatEntry(ChatHistoryAgentThread thread)
        {
            Thread = thread;
            Touch();
        }
        public void Touch() => LastUsedUtc = DateTime.UtcNow;
    }

    private readonly ConcurrentDictionary<Guid, ChatEntry> _cache = new();
    private readonly Lock _lockObj = new();

    public ChatHistoryAgentThread GetOrCreateThread(Guid chatId)
    {
        using (_lockObj.EnterScope())
        {
            var entry = _cache.GetOrAdd(chatId, _ => new ChatEntry(new ChatHistoryAgentThread()));
            entry.Touch();
            return entry.Thread;
        }
    }

    public void Cleanup(DateTime threshold)
    {
        using (_lockObj.EnterScope())
        {
            var expired = GetAllLastUsed()
                .Where(x => x.LastUsedUtc < threshold)
                .ToList();

            foreach (var entry in expired)
            {
                _cache.TryRemove(entry.ChatId, out _);
            }
        }
    }

    private IEnumerable<(Guid ChatId, DateTime LastUsedUtc)> GetAllLastUsed()
    {
        return _cache.Select(kvp => (kvp.Key, kvp.Value.LastUsedUtc));
    }
}