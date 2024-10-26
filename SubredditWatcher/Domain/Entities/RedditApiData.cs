using System.Text.Json.Serialization;

namespace SubredditWatcher.Domain.Entities;

public class RedditApiData
{
    [JsonPropertyName("dist")] public int Dist { get; set; }

    [JsonPropertyName("children")] public List<RedditApiPost> Children { get; set; }
}