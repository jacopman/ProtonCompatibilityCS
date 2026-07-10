using System.Text.Json.Serialization;
namespace JsonResponseModels;

/// <summary>
///    Represents the response returned from the Steam API as a SteamApiResponse object.
/// </summary>
public class SteamApiResponse
{
    /// <summary>
    ///   Represents the data returned from the Steam API as a SteamResponseData object.
    /// </summary>
    [JsonPropertyName("response")]
    public required SteamResponseData Response { get; init; }
}