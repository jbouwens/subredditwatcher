using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.Infrastructure.Repositories.Interfaces;

namespace SubredditWatcher.Tests.Mocks;

public class MockRedditRepository : IRedditRepository
{
    private readonly ILogger<MockRedditRepository> _logger;
    private readonly Queue<(string Subreddit, string ResponseContent)> _responses = new();
    private readonly string _testDataDirectory;

    public MockRedditRepository(string testDataDirectory, ILogger<MockRedditRepository> logger)
    {
        _logger = logger;
        _testDataDirectory = testDataDirectory;
        LoadResponses(testDataDirectory);
    }

    public Task<RedditApiResult> FetchNewPostsAsync(string subreddit)
    {
        _logger.LogInformation($"FetchNewPostsAsync called for subreddit: {subreddit}");

        if (!_responses.Any())
        {
            _logger.LogError($"No more responses available in {_testDataDirectory}");
            throw new InvalidOperationException("No more recorded responses available");
        }

        var (expectedSubreddit, responseContent) = _responses.Dequeue();

        _logger.LogInformation($"Returning response for subreddit: {expectedSubreddit}");

        if (expectedSubreddit != subreddit)
            _logger.LogWarning($"Subreddit mismatch: Expected {expectedSubreddit}, got request for {subreddit}");

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<RedditApiResponse>(responseContent, options);

            if (response == null)
            {
                _logger.LogError("Failed to deserialize response content");
                throw new JsonException("Failed to deserialize response content");
            }

            return Task.FromResult(new RedditApiResult
            {
                Response = response,
                RateLimitUsed = 0,
                RateLimitRemaining = 100,
                RateLimitReset = 60
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing response content");
            throw;
        }
    }

    private void LoadResponses(string testDataDirectory)
    {
        _logger.LogInformation($"Loading responses from directory: {testDataDirectory}");

        if (!Directory.Exists(testDataDirectory))
        {
            _logger.LogError($"Test data directory not found: {testDataDirectory}");
            throw new DirectoryNotFoundException($"Test data directory not found: {testDataDirectory}");
        }

        var files = Directory
            .GetFiles(testDataDirectory, "*.json")
            .OrderBy(f => f)
            .ToList();

        _logger.LogInformation($"Found {files.Count} response files");

        foreach (var file in files)
            try
            {
                _logger.LogInformation($"Processing file: {Path.GetFileName(file)}");
                var content = File.ReadAllText(file);
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;

                if (!root.TryGetProperty("Metadata", out var metadata) ||
                    !metadata.TryGetProperty("Subreddit", out var subredditElement))
                {
                    _logger.LogWarning($"Skipping malformed response file (missing metadata): {file}");
                    continue;
                }

                if (!root.TryGetProperty("Response", out var response))
                {
                    _logger.LogWarning($"Skipping malformed response file (missing response): {file}");
                    continue;
                }

                var subreddit = subredditElement.GetString();
                if (string.IsNullOrEmpty(subreddit))
                {
                    _logger.LogWarning($"Skipping malformed response file (invalid subreddit): {file}");
                    continue;
                }

                _responses.Enqueue((
                    subreddit,
                    response.ToString()
                ));

                _logger.LogInformation($"Successfully loaded response for subreddit: {subreddit}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error parsing response file: {file}");
            }

        _logger.LogInformation($"Successfully loaded {_responses.Count} responses");
    }
}