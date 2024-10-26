using SubredditWatcher.Infrastructure.Repositories;

namespace SubredditWatcher.Application.Interfaces;

/// <summary>
///     Handles Reddit authentication.
/// </summary>
public interface IRedditAuthService
{
    /// <summary>
    ///     Performs authentication and returns a token.
    /// </summary>
    Task<TokenResponse> AuthenticateAsync();
}