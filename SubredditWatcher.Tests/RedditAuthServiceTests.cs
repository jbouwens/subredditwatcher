using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SubredditWatcher.Application.Services;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Tests;

[TestClass]
public class RedditAuthServiceTests
{
    private Mock<IBrowserLauncher>? _browserLauncherMock;
    private Mock<IHttpListenerContext>? _contextMock;
    private Mock<IHttpListener>? _httpListenerMock;
    private ILogger<RedditAuthService>? _logger;
    private Mock<Stream>? _outputStreamMock;
    private RedditSettings? _redditSettings;
    private Mock<IHttpListenerRequest>? _requestMock;
    private Mock<IHttpListenerResponse>? _responseMock;
    private Mock<IUserInterface>? _userInterfaceMock;

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
        _userInterfaceMock = new Mock<IUserInterface>();
        _httpListenerMock = new Mock<IHttpListener>();
        _contextMock = new Mock<IHttpListenerContext>();
        _requestMock = new Mock<IHttpListenerRequest>();
        _responseMock = new Mock<IHttpListenerResponse>();
        _outputStreamMock = new Mock<Stream>();
        _browserLauncherMock = new Mock<IBrowserLauncher>();
        _logger = new NullLogger<RedditAuthService>();

        _responseMock.SetupProperty(r => r.ContentLength64);
        _responseMock.Setup(r => r.OutputStream).Returns(_outputStreamMock.Object);
        _contextMock.Setup(c => c.Response).Returns(_responseMock.Object);
        _contextMock.Setup(c => c.Request).Returns(_requestMock.Object);
        _httpListenerMock.Setup(h => h.GetContextAsync()).ReturnsAsync(_contextMock.Object);
    }

    [TestMethod]
    public async Task AuthenticateAsync_ShouldThrowException_WhenStateIsInvalid()
    {
        // Assemble
        var queryString = new NameValueCollection
        {
            { "state", "invalid_state" },
            { "code", "test_code" }
        };
        _requestMock?.Setup(r => r.QueryString).Returns(queryString);
        var authService = new RedditAuthService(
            _redditSettings,
            _logger,
            _userInterfaceMock.Object,
            _httpListenerMock.Object,
            _browserLauncherMock.Object);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => authService.AuthenticateAsync(),
            "Should throw InvalidOperationException when state parameter doesn't match");

        // Assert
        _httpListenerMock.Verify(h => h.Start(), Times.Once);
        _httpListenerMock.Verify(h => h.Stop(), Times.Once);
        _httpListenerMock.Verify(h => h.AddPrefix(_redditSettings.Authentication.RedirectUri), Times.Once);
        _browserLauncherMock.Verify(b => b.OpenUrl(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync_ShouldProceed_WhenStateIsValid()
    {
        // Assemble
        var queryString = new NameValueCollection
        {
            { "state", "test_state" },
            { "code", "test_code" }
        };
        _requestMock.Setup(r => r.QueryString).Returns(queryString);
        var authService = new RedditAuthService(
            _redditSettings,
            _logger,
            _userInterfaceMock.Object,
            _httpListenerMock.Object,
            _browserLauncherMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => authService.AuthenticateAsync(),
            "Should attempt token exchange when state is valid");

        // Assert
        Assert.IsTrue(exception.Message.Contains("Error exchanging code for tokens"));
        Assert.IsTrue(exception.Message.Contains("Unauthorized"));
        _httpListenerMock.Verify(h => h.Start(), Times.Once);
        _httpListenerMock.Verify(h => h.Stop(), Times.Once);
        _browserLauncherMock.Verify(b => b.OpenUrl(It.IsAny<string>()), Times.Once);
        _userInterfaceMock.Verify(u =>
                u.WriteLine(It.Is<string>(s => s.Contains("Token exchange"))),
            Times.AtLeastOnce);
    }
}