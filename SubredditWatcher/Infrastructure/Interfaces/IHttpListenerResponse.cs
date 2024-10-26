namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     HTTP response with content length and output stream.
/// </summary>
public interface IHttpListenerResponse
{
    /// <summary>
    ///     Response content length.
    /// </summary>
    long ContentLength64 { get; set; }

    /// <summary>
    ///     Stream for response output.
    /// </summary>
    Stream OutputStream { get; }
}