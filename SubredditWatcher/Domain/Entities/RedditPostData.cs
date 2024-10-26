using System.Text.Json.Serialization;

namespace SubredditWatcher.Domain.Entities;

public class RedditPostData
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("author")] public required string Author { get; set; }

    [JsonPropertyName("title")] public required string Title { get; set; }

    [JsonPropertyName("ups")] public int Ups { get; set; }

    [JsonPropertyName("created_utc")] public double CreatedUtc { get; set; }
}