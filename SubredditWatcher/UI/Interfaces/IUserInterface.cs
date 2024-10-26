namespace SubredditWatcher.UI.Interfaces;

/// <summary>
///     Provides methods for console input and output operations.
/// </summary>
public interface IUserInterface
{
    /// <summary>
    ///     Writes a message to the console with a newline.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void WriteLine(string message);

    /// <summary>
    ///     Writes a message to the console without a newline.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void Write(string message);

    /// <summary>
    ///     Reads a line of input from the console.
    /// </summary>
    /// <returns>The input string or null.</returns>
    string? ReadLine();
}