using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.Domain.Services;
using SubredditWatcher.Infrastructure.Repositories.Interfaces;

namespace SubredditWatcher.Infrastructure.Repositories;

/// <inheritdoc />
public class RedditRepository : IRedditRepository
{
    private static int _sequenceNumber;
    private static readonly object LockObject = new();
    private static DateTime? _sessionStartTime;
    private readonly ILogger<RedditRepository> _logger;
    private readonly string _responseCacheDirectory;
    private readonly TokenManager _tokenManager;

    public RedditRepository(
        TokenManager tokenManager,
        ILogger<RedditRepository> logger)
    {
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_sessionStartTime == null)
        {
            _sessionStartTime = DateTime.UtcNow;
            _sequenceNumber = 0;
        }

        var sessionTimestamp = _sessionStartTime?.ToString("yyyyMMdd_HHmmss");
        _responseCacheDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "ApiResponses",
            sessionTimestamp);

        if (!Directory.Exists(_responseCacheDirectory)) Directory.CreateDirectory(_responseCacheDirectory);
    }

    /// <inheritdoc />
    public async Task<RedditApiResult> FetchNewPostsAsync(string subreddit)
    {
        if (string.IsNullOrWhiteSpace(subreddit))
            throw new ArgumentException("Subreddit name cannot be null or empty.", nameof(subreddit));

        try
        {
            var client = await _tokenManager.GetHttpClientAsync();
            var url = $"https://oauth.reddit.com/r/{subreddit}/new?limit=100&sort=new";

            _logger.LogInformation($"Fetching new posts from subreddit: {subreddit}");

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error fetching posts from subreddit '{subreddit}': {errorContent}");
                throw new HttpRequestException($"Error fetching posts: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            //await SaveResponseToFileAsync(subreddit, responseContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var redditApiResponse = JsonSerializer.Deserialize<RedditApiResponse>(responseContent, options);

            if (redditApiResponse == null)
            {
                _logger.LogError($"Failed to deserialize Reddit API response for subreddit '{subreddit}'.");
                throw new JsonException("Failed to deserialize Reddit API response.");
            }

            _logger.LogInformation($"Successfully fetched posts from subreddit: {subreddit}");

            var rateLimitUsed = TryGetHeaderValue(response.Headers, "X-RateLimit-Used", 0);
            var rateLimitRemaining = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", 0);
            var rateLimitReset = TryGetHeaderValue(response.Headers, "X-RateLimit-Reset", 0);

            return new RedditApiResult
            {
                Response = redditApiResponse,
                RateLimitUsed = rateLimitUsed,
                RateLimitRemaining = rateLimitRemaining,
                RateLimitReset = rateLimitReset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while fetching posts from subreddit '{subreddit}'.");
            throw;
        }
    }

    /// <summary>
    ///     Saves the API response to a file for testing. May be removed in the future.
    /// </summary>
    /// <param name="subreddit">The subreddit name.</param>
    /// <param name="responseContent">The API response content.</param>
    /// <returns>A task representing the save operation.</returns>
    private async Task SaveResponseToFileAsync(string subreddit, string responseContent)
    {
        try
        {
            int currentSequence;
            lock (LockObject)
            {
                currentSequence = _sequenceNumber++;
            }

            var sequenceStr = currentSequence.ToString("D4");

            var fileName = Path.Combine(_responseCacheDirectory, $"{sequenceStr}_{subreddit}.json");

            var responseData = new
            {
                Metadata = new
                {
                    SequenceNumber = currentSequence,
                    Timestamp = DateTime.UtcNow,
                    Subreddit = subreddit
                },
                Response = JsonDocument.Parse(responseContent)
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var formattedJson = JsonSerializer.Serialize(responseData, options);

            await File.WriteAllTextAsync(fileName, formattedJson);
            _logger.LogInformation($"Saved API response to file: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save API response to file");
        }
    }

    private int TryGetHeaderValue(HttpHeaders headers, string headerName, int defaultValue)
    {
        if (headers.TryGetValues(headerName, out var values))
        {
            var headerValue = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue) && float.TryParse(headerValue, out var parsedFloat))
                return (int)parsedFloat;
        }

        return defaultValue;
    }
}