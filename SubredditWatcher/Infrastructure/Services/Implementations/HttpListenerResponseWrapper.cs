using System.Net;
using SubredditWatcher.Infrastructure.Interfaces;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class HttpListenerResponseWrapper(HttpListenerResponse response) : IHttpListenerResponse
{
    /// <inheritdoc />
    public long ContentLength64
    {
        get => response.ContentLength64;
        set => response.ContentLength64 = value;
    }

    /// <inheritdoc />
    public Stream OutputStream => response.OutputStream;
}