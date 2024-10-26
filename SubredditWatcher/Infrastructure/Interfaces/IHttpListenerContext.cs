namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     Represents the context of an HTTP request and response.
/// </summary>
public interface IHttpListenerContext
{
    /// <summary>
    ///     Gets the incoming HTTP request.
    /// </summary>
    IHttpListenerRequest Request { get; }

    /// <summary>
    ///     Gets the HTTP response to send back.
    /// </summary>
    IHttpListenerResponse Response { get; }
}