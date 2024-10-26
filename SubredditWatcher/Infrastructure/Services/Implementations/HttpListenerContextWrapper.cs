using System.Net;
using SubredditWatcher.Infrastructure.Interfaces;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class HttpListenerContextWrapper(HttpListenerContext context) : IHttpListenerContext
{
    /// <inheritdoc />
    public IHttpListenerRequest Request => new HttpListenerRequestWrapper(context.Request);

    /// <inheritdoc />
    public IHttpListenerResponse Response => new HttpListenerResponseWrapper(context.Response);
}