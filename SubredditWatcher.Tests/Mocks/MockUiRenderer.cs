using Spectre.Console;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Tests.Mocks;

public class MockUiRenderer : IUiRenderer
{
    public int UpdateStatsPanelCallCount { get; private set; }
    public List<string> EventLogMessages { get; } = new();

    public void UpdateStatsPanel(
        int cycleCount,
        int newPosts,
        int newUsers,
        int rateLimitUsed,
        int rateLimitRemaining,
        int rateLimitReset,
        List<UserPostData> userPostCount,
        List<PostData> postsWithUpvotes)
    {
        UpdateStatsPanelCallCount++;
    }

    public void UpdateEventLogPanel(string message)
    {
        EventLogMessages.Add(message);
    }

    public Task StartUiLoopAsync(Func<LiveDisplayContext, Task> action)
    {
        return action(null);
    }
}