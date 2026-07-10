using System.Text.Json.Serialization;

namespace JsonResponseModels;

/// <summary>
/// Represents the response from the ProtonDB API, containing information about the compatibility tier of a game on Linux.
/// </summary>
public class ProtonDbResponse
{
    /// <summary>
    /// Gets or sets the compatibility tier of the game on Linux, as reported by ProtonDB.
    /// </summary>
    [JsonPropertyName("tier")]
    public required string Tier { get; set; }
}