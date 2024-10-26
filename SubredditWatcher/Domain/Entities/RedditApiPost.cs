using System.Text.Json.Serialization;

namespace SubredditWatcher.Domain.Entities;

public class RedditApiPost
{
    [JsonPropertyName("data")] public RedditPostData Data { get; set; }
}