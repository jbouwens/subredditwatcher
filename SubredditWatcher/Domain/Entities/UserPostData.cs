namespace SubredditWatcher.Domain.Entities;

public class UserPostData
{
    public required string Username { get; set; }
    public required string Subreddit { get; set; }
    public int PostCount { get; set; }
}