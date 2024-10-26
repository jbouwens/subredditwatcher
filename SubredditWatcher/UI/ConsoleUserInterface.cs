using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.UI;

/// <inheritdoc />
public class ConsoleUserInterface : IUserInterface
{
    /// <inheritdoc />
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    /// <inheritdoc />
    public void Write(string message)
    {
        Console.Write(message);
    }

    /// <inheritdoc />
    public string? ReadLine()
    {
        return Console.ReadLine();
    }
}