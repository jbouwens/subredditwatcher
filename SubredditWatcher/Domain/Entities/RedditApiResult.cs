namespace SubredditWatcher.Domain.Entities;

public class RedditApiResult
{
    public required RedditApiResponse Response { get; set; }
    public int RateLimitUsed { get; set; }
    public int RateLimitRemaining { get; set; }
    public int RateLimitReset { get; set; }
}