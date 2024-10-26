using Spectre.Console;
using SubredditWatcher.Domain.Entities;

namespace SubredditWatcher.UI.Interfaces;

/// <summary>
///     Renders UI updates for the application.
/// </summary>
public interface IUiRenderer
{
    /// <summary>
    ///     Updates the stats panel with monitoring data.
    /// </summary>
    void UpdateStatsPanel(int cycleCount, int newPosts, int newUsers, int rateLimitUsed, int rateLimitRemaining,
        int rateLimitReset, List<UserPostData> userPostCount, List<PostData> postsWithUpvotes);

    /// <summary>
    ///     Updates the event log panel with a message.
    /// </summary>
    void UpdateEventLogPanel(string message);

    /// <summary>
    ///     Starts the UI loop with a specified action.
    /// </summary>
    Task StartUiLoopAsync(Func<LiveDisplayContext, Task> action);
}