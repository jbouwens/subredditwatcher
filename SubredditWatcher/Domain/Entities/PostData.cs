namespace SubredditWatcher.Domain.Entities;

public class PostData
{
    public required string PostId { get; set; }
    public required string Title { get; set; }
    public int Upvotes { get; set; }
    public int InitialUpvotes { get; set; }
    public required string Subreddit { get; set; }
}