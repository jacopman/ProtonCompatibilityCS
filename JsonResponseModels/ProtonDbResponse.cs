using System.Text.Json.Serialization;

namespace JsonResponseModels;

public class ProtonDbResponse
{
    [JsonPropertyName("tier")]
    public required string Tier { get; set; }
}