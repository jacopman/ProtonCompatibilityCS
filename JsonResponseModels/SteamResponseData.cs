using System.Text.Json.Serialization;

namespace JsonResponseModels;

/// <summary>
///     Represents the data returned from the Steam API.
/// </summary>
public class SteamResponseData
{
    /// <summary>
    ///    Represents the total number of games returned in the response.
    /// </summary>
    [JsonPropertyName("game_count")]
    public int GameCount { get; init; }
    
    /// <summary>
    ///    Represents the list of games response represented by the SteamGame class.
    /// </summary>
    [JsonPropertyName("games")]
    public required List<SteamGame> Games { get; init; }
}