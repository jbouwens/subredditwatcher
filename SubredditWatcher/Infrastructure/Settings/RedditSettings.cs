namespace SubredditWatcher.Infrastructure.Settings;

/// <summary>
///     Configuration for Reddit API settings.
/// </summary>
public class RedditSettings
{
    /// <summary>
    ///     Reddit authentication details.
    /// </summary>
    public required AuthenticationSettings Authentication { get; set; }

    /// <summary>
    ///     List of subreddits to monitor.
    /// </summary>
    public required List<string> Subreddits { get; set; } = new();
}

/// <summary>
///     Reddit API authentication settings.
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    ///     Reddit client ID.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    ///     Reddit client secret.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    ///     Redirect URI for OAuth callbacks.
    /// </summary>
    public required string RedirectUri { get; set; }

    /// <summary>
    ///     User agent string for API requests.
    /// </summary>
    public required string UserAgent { get; set; }

    /// <summary>
    ///     OAuth scope for API permissions.
    /// </summary>
    public required string Scope { get; set; }

    /// <summary>
    ///     State parameter for OAuth verification.
    /// </summary>
    public required string State { get; set; }
}