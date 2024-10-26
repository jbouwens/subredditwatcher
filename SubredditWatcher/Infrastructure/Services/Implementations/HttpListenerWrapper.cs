using System.Net;
using SubredditWatcher.Infrastructure.Interfaces;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class HttpListenerWrapper : IHttpListener
{
    private readonly HttpListener _listener = new();

    /// <inheritdoc />
    public void Start()
    {
        _listener.Start();
    }

    /// <inheritdoc />
    public void Stop()
    {
        _listener.Stop();
    }

    /// <inheritdoc />
    public void AddPrefix(string uriPrefix)
    {
        _listener.Prefixes.Add(uriPrefix);
    }

    /// <inheritdoc />
    public async Task<IHttpListenerContext> GetContextAsync()
    {
        var context = await _listener.GetContextAsync();
        return new HttpListenerContextWrapper(context);
    }
}