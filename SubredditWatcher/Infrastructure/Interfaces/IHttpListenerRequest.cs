using System.Collections.Specialized;

namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     Represents an HTTP request, providing access to the query string.
/// </summary>
public interface IHttpListenerRequest
{
    /// <summary>
    ///     Gets the collection of query string parameters.
    /// </summary>
    NameValueCollection QueryString { get; }
}