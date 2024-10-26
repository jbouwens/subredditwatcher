namespace SubredditWatcher.Infrastructure.Interfaces;

/// <summary>
///     Launches a web browser to open a specified URL.
/// </summary>
public interface IBrowserLauncher
{
    /// <summary>
    ///     Opens the specified URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenUrl(string url);
}