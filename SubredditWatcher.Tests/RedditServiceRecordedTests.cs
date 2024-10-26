using Microsoft.Extensions.Logging;
using SubredditWatcher.Application.Services;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.Tests.Mocks;

namespace SubredditWatcher.Tests;

[TestClass]
public class RedditServiceRecordedTests
{
    private ILogger<RedditService> _logger;
    private ILogger<MockRedditRepository> _mockRepositoryLogger;

    [TestInitialize]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        _logger = loggerFactory.CreateLogger<RedditService>();
        _mockRepositoryLogger = loggerFactory.CreateLogger<MockRedditRepository>();
    }

    [TestMethod]
    public async Task ProcessMultipleSubreddits_ShouldTrackNewPostsAndUsers()
    {
        // Arrange
        var testUiRenderer = new MockUiRenderer();
        var redditSettings = new RedditSettings
        {
            Subreddits = new List<string> { "legaladvice", "askreddit", "relationshipadvice" },
            Authentication = new AuthenticationSettings
            {
                ClientId = "test_client_id",
                ClientSecret = "test_client_secret",
                RedirectUri = "http://localhost:32939/",
                UserAgent = "SubredditWatcherTest/1.0",
                Scope = "identity,read",
                State = "test_state"
            }
        };

        var projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

        var testDataDirectory = Path.Combine(projectDirectory, "TestData",
            "ProcessMultipleSubreddits_ShouldTrackNewPostsAndUsers");

        var redditRepository = new MockRedditRepository(testDataDirectory, _mockRepositoryLogger);

        var fixedSessionStartTime = new DateTime(2024, 10, 26, 0, 0, 0, DateTimeKind.Utc);

        var redditService = new RedditService(
            redditSettings,
            _logger,
            testUiRenderer,
            redditRepository,
            fixedSessionStartTime);

        // Act
        foreach (var subreddit in redditSettings.Subreddits) await redditService.ProcessSubredditAsync(subreddit);

        // Assert
        Assert.IsTrue(testUiRenderer.EventLogMessages.Count > 0,
            $"Should have logged events. Current message count: {testUiRenderer.EventLogMessages.Count}");
        // add more asserts ...
    }
}