using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SubredditWatcher.Application.Interfaces;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.Infrastructure.Repositories.Interfaces;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Application.Services;

/// <inheritdoc />
public class RedditService : IRedditService
{
    private readonly ILogger<RedditService> _logger;
    private readonly ConcurrentDictionary<string, PostData> _postsWithUpvotes = new();
    private readonly IRedditRepository _redditRepository;
    private readonly RedditSettings _redditSettings;

    private readonly DateTime _sessionStartTime;
    private readonly IUiRenderer _uiRenderer;
    private readonly ConcurrentDictionary<string, UserPostData> _userPostDataList = new();
    private int _cycleCount;
    private int _rateLimitRemaining;
    private int _rateLimitReset;

    private int _rateLimitUsed;

    private int _totalNewPosts;
    private int _totalNewUsers;

    // Modified constructor to accept an optional sessionStartTime
    public RedditService(
        RedditSettings redditSettings,
        ILogger<RedditService> logger,
        IUiRenderer uiRenderer,
        IRedditRepository redditRepository,
        DateTime? sessionStartTime = null)
    {
        _redditSettings = redditSettings;
        _logger = logger;
        _uiRenderer = uiRenderer;
        _redditRepository = redditRepository;
        _sessionStartTime = sessionStartTime ?? DateTime.UtcNow;

        _logger.LogInformation("Session start time set to {SessionStartTime} UTC.",
            _sessionStartTime.ToUniversalTime());
    }

    /// <inheritdoc />
    public async Task StartMonitoringAsync()
    {
        try
        {
            await _uiRenderer.StartUiLoopAsync(async ctx =>
            {
                while (true)
                {
                    var cycleNewPosts = 0;
                    var cycleNewUsers = 0;

                    var tasks = _redditSettings.Subreddits.Select(async subreddit =>
                    {
                        var (newPosts, newUsers) = await ProcessSubredditAsync(subreddit);
                        Interlocked.Add(ref cycleNewPosts, newPosts);
                        Interlocked.Add(ref cycleNewUsers, newUsers);
                    });

                    await Task.WhenAll(tasks);

                    Interlocked.Add(ref _totalNewPosts, cycleNewPosts);
                    Interlocked.Add(ref _totalNewUsers, cycleNewUsers);
                    Interlocked.Increment(ref _cycleCount);

                    _uiRenderer.UpdateStatsPanel(
                        _cycleCount,
                        _totalNewPosts,
                        _totalNewUsers,
                        _rateLimitUsed,
                        _rateLimitRemaining,
                        _rateLimitReset,
                        _userPostDataList.Values.ToList(),
                        _postsWithUpvotes.Values.ToList());

                    ctx.Refresh();

                    await Task.Delay(1000);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in StartMonitoringAsync.");
            throw;
        }
    }

    /// <summary>
    ///     Processes new posts from a subreddit with thread-safe updates to post and user data.
    /// </summary>
    /// <param name="subreddit">The subreddit name to process</param>
    /// <returns>Count of new posts and users found</returns>
    public async Task<(int NewPosts, int NewUsers)> ProcessSubredditAsync(string subreddit)
    {
        try
        {
            var apiResult = await _redditRepository.FetchNewPostsAsync(subreddit);

            _rateLimitUsed = apiResult.RateLimitUsed;
            _rateLimitRemaining = apiResult.RateLimitRemaining;
            _rateLimitReset = apiResult.RateLimitReset;

            var postData = apiResult.Response;

            if (postData.Data.Children == null || !postData.Data.Children.Any())
            {
                _logger.LogWarning($"No posts returned for subreddit '{subreddit}'.");
                return (0, 0);
            }

            _logger.LogInformation($"Processing {postData.Data.Children.Count} posts from r/{subreddit}.");

            var newPostCount = 0;
            var newUserCount = 0;

            foreach (var post in postData.Data.Children)
            {
                if (post?.Data == null) continue;

                var postCreatedUtc = DateTimeOffset.FromUnixTimeSeconds((long)post.Data.CreatedUtc).UtcDateTime;
                _logger.LogDebug($"Post '{post.Data.Title}' created at {postCreatedUtc:O} UTC.");

                if (postCreatedUtc < _sessionStartTime)
                {
                    _logger.LogDebug("Skipping post (before session start time).");
                    continue;
                }

                var postId = post.Data.Id;
                if (!_postsWithUpvotes.TryGetValue(postId, out var currentPost))
                {
                    _logger.LogInformation($"New post detected: {postId}");

                    var newPost = new PostData
                    {
                        PostId = postId,
                        Title = post.Data.Title,
                        Upvotes = post.Data.Ups,
                        InitialUpvotes = post.Data.Ups,
                        Subreddit = subreddit
                    };

                    if (_postsWithUpvotes.TryAdd(postId, newPost))
                    {
                        newPostCount++;
                        _uiRenderer.UpdateEventLogPanel(
                            $"🆕 New post by {post.Data.Author} in r/{subreddit}: '{TruncateString(post.Data.Title, 50)}' ({post.Data.Ups} upvotes)");

                        var added = false;
                        var userData = _userPostDataList.AddOrUpdate(
                            post.Data.Author,
                            _ =>
                            {
                                added = true;
                                return new UserPostData
                                {
                                    Username = post.Data.Author,
                                    Subreddit = subreddit,
                                    PostCount = 1
                                };
                            },
                            (_, existing) =>
                            {
                                existing.PostCount++;
                                return existing;
                            });

                        if (added)
                        {
                            newUserCount++;
                            _uiRenderer.UpdateEventLogPanel(
                                $"👤 New user alert! {post.Data.Author} makes their debut in r/{subreddit}!");
                        }
                        else
                        {
                            _uiRenderer.UpdateEventLogPanel(
                                $"{post.Data.Author} now has {userData.PostCount} posts in r/{subreddit}");
                        }
                    }
                }
                else if (currentPost.Upvotes != post.Data.Ups)
                {
                    _postsWithUpvotes.AddOrUpdate(
                        postId,
                        currentPost,
                        (_, existing) =>
                        {
                            var upvoteDifference = post.Data.Ups - existing.Upvotes;
                            var direction = upvoteDifference > 0 ? "gained" : "lost";

                            _uiRenderer.UpdateEventLogPanel(
                                $"⬆️ '{TruncateString(existing.Title, 50)}' {direction} {Math.Abs(upvoteDifference)} votes (now at {post.Data.Ups})");

                            existing.Upvotes = post.Data.Ups;
                            return existing;
                        });
                }
            }

            _logger.LogInformation(
                $"Subreddit '{subreddit}' processing complete. New Posts: {newPostCount}, New Users: {newUserCount}");

            return (newPostCount, newUserCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing subreddit '{subreddit}'.");
            _uiRenderer.UpdateEventLogPanel($"⚠️ Error processing subreddit '{subreddit}': {ex.Message}");
            return (0, 0);
        }
    }

    private string TruncateString(string input, int maxLength)
    {
        return input.Length <= maxLength ? input : input[..(maxLength - 3)] + "...";
    }
}