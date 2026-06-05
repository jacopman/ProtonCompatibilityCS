using System.Text.Json.Serialization;

namespace JsonResponseModels;
public class SteamResponseData
{
    [JsonPropertyName("game_count")]
    public int GameCount { get; set; }

    [JsonPropertyName("games")]
    public required List<SteamGame> Games { get; set; }
}