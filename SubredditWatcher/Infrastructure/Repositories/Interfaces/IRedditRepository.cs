using SubredditWatcher.Domain.Entities;

namespace SubredditWatcher.Infrastructure.Repositories.Interfaces;

/// <summary>
///     Fetches new posts from a subreddit.
/// </summary>
public interface IRedditRepository
{
    /// <summary>
    ///     Gets new posts from a subreddit.
    /// </summary>
    Task<RedditApiResult> FetchNewPostsAsync(string subreddit);
}