namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     Handles HTTP requests by wrapping an HTTP listener.
/// </summary>
public interface IHttpListener
{
    /// <summary>
    ///     Starts listening for HTTP requests.
    /// </summary>
    void Start();

    /// <summary>
    ///     Stops listening for HTTP requests.
    /// </summary>
    void Stop();

    /// <summary>
    ///     Adds a URI prefix to listen for incoming requests.
    /// </summary>
    /// <param name="uriPrefix">The URI prefix to add.</param>
    void AddPrefix(string uriPrefix);

    /// <summary>
    ///     Waits for and returns the next HTTP request context.
    /// </summary>
    /// <returns>The next incoming HTTP request context.</returns>
    Task<IHttpListenerContext> GetContextAsync();
}