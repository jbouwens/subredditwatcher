using System.Collections.Specialized;
using System.Net;
using SubredditWatcher.Infrastructure.Interfaces;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class HttpListenerRequestWrapper(HttpListenerRequest request) : IHttpListenerRequest
{
    /// <inheritdoc />
    public NameValueCollection QueryString => request.QueryString;
}