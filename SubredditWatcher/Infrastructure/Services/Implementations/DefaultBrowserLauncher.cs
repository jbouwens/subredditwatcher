using System.Diagnostics;
using SubredditWatcher.Infrastructure.Interfaces;

namespace SubredditWatcher.Infrastructure.Services.Implementations;

/// <inheritdoc />
public class DefaultBrowserLauncher : IBrowserLauncher
{
    /// <inheritdoc />
    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // ignored
        }
    }
}