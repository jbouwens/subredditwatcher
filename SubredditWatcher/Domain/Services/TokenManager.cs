using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Repositories;
using SubredditWatcher.Infrastructure.Settings;

namespace SubredditWatcher.Domain.Services;

public class TokenManager
{
    private readonly HttpClient _httpClient;
    private readonly RedditSettings _redditSettings;
    private readonly ITokenStorageService _tokenStorageService;
    private DateTime _accessTokenExpiration;
    private string? _refreshToken;

    public string? AccessToken;

    public TokenManager(RedditSettings redditSettings, ITokenStorageService tokenStorageService, HttpClient httpClient)
    {
        _redditSettings = redditSettings;
        _tokenStorageService = tokenStorageService;
        _httpClient = httpClient;
    }

    public void Initialize(TokenResponse tokenResponse)
    {
        AccessToken = tokenResponse.AccessToken;
        _refreshToken = tokenResponse.RefreshToken;
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
    }

    public async Task<HttpClient> GetHttpClientAsync()
    {
        if (string.IsNullOrEmpty(AccessToken) || DateTime.UtcNow >= _accessTokenExpiration)
            await RefreshAccessTokenAsync();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        client.DefaultRequestHeaders.Add("User-Agent", _redditSettings.Authentication.UserAgent);
        return client;
    }

    private async Task RefreshAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
            throw new InvalidOperationException("Refresh token is not available.");

        var client = _httpClient;

        var authHeader = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{_redditSettings.Authentication.ClientId}:{_redditSettings.Authentication.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        client.DefaultRequestHeaders.Add("User-Agent", _redditSettings.Authentication.UserAgent);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", _refreshToken)
        });

        var tokenResponse = await client.PostAsync("https://www.reddit.com/api/v1/access_token", content);
        var responseContent = await tokenResponse.Content.ReadAsStringAsync();

        if (!tokenResponse.IsSuccessStatusCode)
            throw new Exception($"Error refreshing access token: {responseContent}");

        var tokenData = JsonSerializer.Deserialize<TokenResponse>(responseContent);
        if (tokenData == null) throw new Exception("Failed to deserialize refreshed token response");

        AccessToken = tokenData.AccessToken;
        _refreshToken = tokenData.RefreshToken ?? _refreshToken;
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        await _tokenStorageService.SaveTokenAsync(new TokenResponse
        {
            AccessToken = AccessToken,
            RefreshToken = _refreshToken,
            ExpiresIn = tokenData.ExpiresIn,
            Scope = tokenData.Scope,
            TokenType = tokenData.TokenType
        });
    }
}