using SubredditWatcher.Infrastructure.Repositories;

namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     Manages saving and loading tokens.
/// </summary>
public interface ITokenStorageService
{
    /// <summary>
    ///     Saves a token.
    /// </summary>
    Task SaveTokenAsync(TokenResponse tokenResponse);

    /// <summary>
    ///     Loads a token.
    /// </summary>
    Task<TokenResponse?> LoadTokenAsync();
}