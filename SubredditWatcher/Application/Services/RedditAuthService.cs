using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubredditWatcher.Application.Interfaces;
using SubredditWatcher.Infrastructure.Interfaces;
using SubredditWatcher.Infrastructure.Repositories;
using SubredditWatcher.Infrastructure.Services.Implementations;
using SubredditWatcher.Infrastructure.Settings;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.Application.Services;

/// <inheritdoc />
public class RedditAuthService(
    RedditSettings? redditSettings,
    ILogger<RedditAuthService> logger,
    IUserInterface ui,
    IHttpListener httpListener,
    IBrowserLauncher? browserLauncher = null)
    : IRedditAuthService
{
    private readonly AuthenticationSettings _authSettings = redditSettings?.Authentication;
    private readonly IBrowserLauncher _browserLauncher = browserLauncher ?? new DefaultBrowserLauncher();

    /// <inheritdoc />
    public async Task<TokenResponse> AuthenticateAsync()
    {
        logger.LogInformation("Starting authentication process.");

        var authorizationUrl =
            $"https://www.reddit.com/api/v1/authorize?client_id={_authSettings.ClientId}&response_type=code&state={_authSettings.State}&redirect_uri={_authSettings.RedirectUri}&duration=permanent&scope={_authSettings.Scope}";

        ui.WriteLine("Opening browser for Reddit authentication...");
        ui.WriteLine("If the browser doesn't open automatically, copy and paste this URL into your browser:");
        ui.WriteLine(authorizationUrl);

        _browserLauncher.OpenUrl(authorizationUrl);

        httpListener.AddPrefix(_authSettings.RedirectUri);
        httpListener.Start();

        var context = await httpListener.GetContextAsync();
        var response = context.Response;
        const string responseString = "<html><body>You can close this window.</body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();

        httpListener.Stop();

        var query = context.Request.QueryString;
        var receivedState = query["state"];
        var code = query["code"];

        if (receivedState != _authSettings.State)
            throw new InvalidOperationException("Invalid state received in the callback.");

        logger.LogInformation("Authentication successful.");
        return await ExchangeCodeForTokensAsync(code);
    }

    private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code)
    {
        ui.WriteLine("Starting token exchange process...");
        using var client = new HttpClient();
        var authHeader =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_authSettings.ClientId}:{_authSettings.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        client.DefaultRequestHeaders.Add("User-Agent", _authSettings.UserAgent);

        ui.WriteLine($"Using User-Agent: {_authSettings.UserAgent}");

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _authSettings.RedirectUri)
        });

        ui.WriteLine($"Sending request to Reddit API with redirect URI: {_authSettings.RedirectUri}");

        try
        {
            var tokenResponse = await client.PostAsync("https://www.reddit.com/api/v1/access_token", content);
            var responseContent = await tokenResponse.Content.ReadAsStringAsync();
            ui.WriteLine($"Token exchange response status: {tokenResponse.StatusCode}");
            ui.WriteLine($"Token exchange response: {responseContent}");

            if (!tokenResponse.IsSuccessStatusCode)
                throw new Exception(
                    $"Error exchanging code for tokens. Status code: {tokenResponse.StatusCode}, Content: {responseContent}");

            var tokenResult = JsonSerializer.Deserialize<TokenResponse>(responseContent);
            if (tokenResult == null) throw new Exception("Failed to deserialize token response");

            ui.WriteLine("Token exchange successful!");
            return tokenResult;
        }
        catch (Exception ex)
        {
            ui.WriteLine($"Exception occurred during token exchange: {ex.Message}");
            throw;
        }
    }
}