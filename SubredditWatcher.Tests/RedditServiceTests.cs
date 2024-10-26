using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SubredditWatcher.Application.Services;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.Infrastructure.Repositories.Interfaces;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.Tests.Mocks;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Tests;

[TestClass]
public class RedditServiceTests
{
    private ILogger<RedditService> _logger;
    private Mock<IRedditRepository> _redditRepositoryMock;
    private RedditSettings _redditSettings;
    private Mock<IUiRenderer> _uiRendererMock;

    [TestInitialize]
    public void Setup()
    {
        _redditSettings = new RedditSettings
        {
            Authentication = new AuthenticationSettings
            {
                ClientId = "test_client_id",
                ClientSecret = "test_client_secret",
                RedirectUri = "http://localhost:32939/",
                UserAgent = "SubredditWatcherTest/1.0",
                Scope = "identity,read",
                State = "test_state"
            },
            Subreddits = new List<string> { "testsubreddit" }
        };
        _redditRepositoryMock = new Mock<IRedditRepository>();
        _uiRendererMock = new Mock<IUiRenderer>();
        _logger = new NullLogger<RedditService>();
    }

    [TestMethod]
    public async Task StartMonitoringAsync_ShouldLogInformation()
    {
        // Assemble
        var testUiRenderer = new MockUiRenderer();
        var redditService = new RedditService(
            _redditSettings,
            _logger,
            testUiRenderer,
            _redditRepositoryMock.Object);

        // Act
        var monitoringTask = redditService.StartMonitoringAsync();
        await Task.Delay(100);

        // Assert
        Assert.IsTrue(testUiRenderer.UpdateStatsPanelCallCount > 0, "UpdateStatsPanel should have been called.");
        Assert.IsTrue(testUiRenderer.EventLogMessages.Count > 0, "Event log should have messages.");
    }

    [TestMethod]
    public async Task ProcessSubredditAsync_ShouldReturnValidResult()
    {
        // Assemble
        _redditRepositoryMock.Setup(r => r.FetchNewPostsAsync(It.IsAny<string>())).ReturnsAsync(new RedditApiResult
        {
            Response = new RedditApiResponse { Data = new RedditApiData() },
            RateLimitRemaining = 100
        });
        var redditService =
            new RedditService(_redditSettings, _logger, _uiRendererMock.Object, _redditRepositoryMock.Object);

        // Act
        var (newPosts, newUsers) = await redditService.ProcessSubredditAsync("testsubreddit");

        // Assert
        Assert.AreEqual(0, newPosts);
        Assert.AreEqual(0, newUsers);
    }
}