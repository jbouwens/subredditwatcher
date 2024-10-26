using System.Text.RegularExpressions;
using Spectre.Console;
using SubredditWatcher.Domain.Entities;
using SubredditWatcher.UI.Interfaces;

namespace SubredditWatcher.UI;

/// <inheritdoc />
public class SpectreUiRenderer : IUiRenderer
{
    private static readonly Regex SanitizationRegex =
        new(
            "[^a-zA-Z0-9\\s.,!?()'\"-]",
            RegexOptions.Compiled
        );

    private readonly Layout _layout = new Layout("Root")
        .SplitColumns(
            new Layout("Left"),
            new Layout("Right")
        );

    private readonly Queue<string> _recentEvents = new(50);

    /// <inheritdoc />
    public Task StartUiLoopAsync(Func<LiveDisplayContext, Task> action)
    {
        return AnsiConsole.Live(_layout).StartAsync(ctx => { return action(ctx); });
    }

    /// <inheritdoc />
    public void UpdateStatsPanel(
        int cycleCount,
        int newPosts,
        int newUsers,
        int rateLimitUsed,
        int rateLimitRemaining,
        int rateLimitReset,
        List<UserPostData> userPostDataList,
        List<PostData> postsWithUpvotes)
    {
        var panel = CreateStatsPanel(
            cycleCount,
            newPosts,
            newUsers,
            rateLimitUsed,
            rateLimitRemaining,
            rateLimitReset,
            userPostDataList,
            postsWithUpvotes
        );
        _layout["Left"].Update(panel);
    }

    /// <inheritdoc />
    public void UpdateEventLogPanel(string message)
    {
        var timeString = DateTime.Now.ToString("HH:mm:ss");
        var sanitizedMessage = SanitizeString(message);
        _recentEvents.Enqueue($"({timeString}) {sanitizedMessage}");
        if (_recentEvents.Count > 100) _recentEvents.Dequeue();
        var panel = CreateEventLogPanel();
        _layout["Right"].Update(panel);
    }

    /// <summary>
    ///     Creates a panel displaying stats like posts, users, and rate limits.
    /// </summary>
    private Panel CreateStatsPanel(
        int cycleCount,
        int newPosts,
        int newUsers,
        int rateLimitUsed,
        int rateLimitRemaining,
        int rateLimitReset,
        List<UserPostData> userPostDataList,
        List<PostData> postsWithUpvotes)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap())
            .AddColumn(new GridColumn().NoWrap());

        grid.AddRow(
            new Panel(new Table()
                    .AddColumn("Metric")
                    .AddColumn("Value")
                    .AddRow("Rate Limit Used", rateLimitUsed.ToString())
                    .AddRow("Remaining", rateLimitRemaining.ToString())
                    .AddRow("Reset (s)", rateLimitReset.ToString())
                    .AddRow("Cycle", cycleCount.ToString())
                    .AddRow("New Posts", newPosts.ToString())
                    .AddRow("New Users", newUsers.ToString()))
                .Header("📊 Stats")
        );

        var topUsers = userPostDataList
            .OrderByDescending(u => u.PostCount)
            .Take(10)
            .ToList();

        var userTable = new Table()
            .AddColumn("Rank")
            .AddColumn("User")
            .AddColumn("Posts");

        foreach (var user in topUsers.Select((u, i) => new { User = u, Index = i }))
            userTable.AddRow(
                $"#{user.Index + 1}",
                TruncateString(user.User.Username, 20),
                user.User.PostCount.ToString()
            );

        grid.AddRow(new Panel(userTable).Header("👥 Top Users"));

        var topPosts = postsWithUpvotes
            .OrderByDescending(p => p.Upvotes)
            .Take(10)
            .ToList();

        var postTable = new Table()
            .AddColumn("Rank")
            .AddColumn("Title")
            .AddColumn("Ups")
            .AddColumn("Δ")
            .AddColumn("Subreddit");

        foreach (var post in topPosts.Select((p, i) => new { Post = p, Index = i }))
            postTable.AddRow(
                $"#{post.Index + 1}",
                TruncateString(post.Post.Title, 50),
                post.Post.Upvotes.ToString(),
                (post.Post.Upvotes - post.Post.InitialUpvotes).ToString("+#;-#;0"),
                post.Post.Subreddit
            );

        grid.AddRow(new Panel(postTable).Header("📝 Top Posts"));

        return new Panel(grid)
            .Header("Reddit Watcher")
            .Expand();
    }

    /// <summary>
    ///     Creates a panel for displaying recent event logs.
    /// </summary>
    private Panel CreateEventLogPanel()
    {
        var content = new Rows(
            _recentEvents
                .Reverse()
                .Select(e => new Markup(Markup.Escape(e)))
        );

        return new Panel(content)
            .Header("📜 Event Log")
            .Expand();
    }

    /// <summary>
    ///     Truncates a string to a specified length.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="maxLength">The maximum length of the output.</param>
    /// <returns>The truncated string.</returns>
    private string TruncateString(string input, int maxLength)
    {
        var sanitized = SanitizeString(input);
        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    /// <summary>
    ///     Sanitizes a string by removing unwanted characters.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The sanitized string.</returns>
    private string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        input = input.Replace('[', '(').Replace(']', ')');

        return SanitizationRegex.Replace(input, "");
    }
}