namespace SubredditWatcher.Application.Interfaces;

/// <summary>
///     Monitors activity on specified subreddits.
/// </summary>
public interface IRedditService
{
    /// <summary>
    ///     Starts concurrent monitoring of multiple subreddits using ConcurrentDictionary
    ///     to track posts and users safely across threads.
    /// </summary>
    Task StartMonitoringAsync();
}