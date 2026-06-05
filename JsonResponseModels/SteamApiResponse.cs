using System.Text.Json.Serialization;
namespace JsonResponseModels;

public class SteamApiResponse
{
    [JsonPropertyName("response")]
    public required SteamResponseData Response { get; set; }
}