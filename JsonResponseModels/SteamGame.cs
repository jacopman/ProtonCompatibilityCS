using System.Text.Json.Serialization;

namespace JsonResponseModels;

public class SteamGame
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}