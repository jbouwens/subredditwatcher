using System.Net;
using Moq;
using Moq.Protected;
using SubredditWatcher.Domain.Services;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Repositories;
using SubredditWatcher.Infrastructure.Settings;

namespace SubredditWatcher.Tests;

[TestClass]
public class TokenManagerTests
{
    private RedditSettings _redditSettings;
    private Mock<ITokenStorageService> _tokenStorageServiceMock;

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
        _tokenStorageServiceMock = new Mock<ITokenStorageService>();
    }

    [TestMethod]
    public async Task GetHttpClientAsync_ShouldRequestNewToken_WhenTokenExpired()
    {
        // Assemble
        var httpClient = CreateMockHttpClient();
        var tokenManager = new TokenManager(_redditSettings, _tokenStorageServiceMock.Object, httpClient);
        tokenManager.Initialize(new TokenResponse
        {
            AccessToken = "initial_token",
            RefreshToken = "test_refresh_token",
            ExpiresIn = -10
        });

        // Act
        var client = await tokenManager.GetHttpClientAsync();

        // Assert
        Assert.IsNotNull(client);
        _tokenStorageServiceMock.Verify(t => t.SaveTokenAsync(It.IsAny<TokenResponse>()), Times.Once);
    }

    [TestMethod]
    public void Initialize_ShouldSetTokensCorrectly()
    {
        // Assemble
        var httpClient = CreateMockHttpClient();
        var tokenManager = new TokenManager(_redditSettings, _tokenStorageServiceMock.Object, httpClient);
        var tokenResponse = new TokenResponse { AccessToken = "test_token", ExpiresIn = 3600 };

        // Act
        tokenManager.Initialize(tokenResponse);

        // Assert
        Assert.AreEqual("test_token", tokenManager.AccessToken);
    }

    private HttpClient CreateMockHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://www.reddit.com/api/v1/access_token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"new_access_token\",\"expires_in\":3600}")
            })
            .Verifiable();
        return new HttpClient(handlerMock.Object);
    }
}