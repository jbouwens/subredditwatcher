using System.Text.Json.Serialization;

namespace SubredditWatcher.Domain.Entities;

public class RedditApiResponse
{
    [JsonPropertyName("data")] public required RedditApiData Data { get; set; }
}