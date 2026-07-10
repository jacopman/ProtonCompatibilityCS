using System.Text.Json.Serialization;

namespace JsonResponseModels;

/// <summary>
///   Represents a game in the user's Steam library, including its AppID and name.
/// </summary>
public class SteamGame
{
    /// <summary>
    ///  Represents the unique identifier for the game in the Steam store.
    /// </summary>
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    /// <summary>
    ///  Represents the name of the game in the user's Steam library.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}