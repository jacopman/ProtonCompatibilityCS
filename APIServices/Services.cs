using JsonResponseModels;
using System.Text.Json;

namespace APIServices;

/// <summary>
///   Represents a service for interacting with the Steam and ProtonDB APIs.
/// </summary>
/// <param name="client">an HttpClient object to Get data from various services</param>
public class ClientService(HttpClient client) : IClientService
{

    private readonly HttpClient _client = client;

    /// <summary>
    /// Fetches the user's Steam library using the Steam Web API by sending an HTTP GET request to the GetOwnedGames endpoint with the provided API key and Steam ID. The method deserializes the JSON response into a list of SteamGame objects, which contain the AppID and name of each game in the user's library. If an error occurs during the API call, the method returns null and prints an error message to the console.
    /// </summary>
    /// <param name="apiKey">The Steam Web API key.</param>
    /// <param name="steamId">The Steam ID of the user.</param>
    /// <returns>A list of SteamGame objects representing the user's library, or null if an error occurs.</returns>
    public async Task<List<SteamGame>> GetSteamLibraryAsync(string apiKey, string steamId)
    {
        string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={apiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=true&format=json";

        try
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            var steamData = JsonSerializer.Deserialize<SteamApiResponse>(jsonString);
            return steamData?.Response?.Games ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Steam library: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Checks the compatibility of a game with Proton by sending an HTTP GET request to the ProtonDB API with the game's AppID. The method deserializes the JSON response to extract the compatibility tier of the game and returns it as a string. If the game is not found in the ProtonDB database, the method returns "native/unknown". If an error occurs during the API call, the method returns "error".
    /// </summary>
    /// <param name="appId">The AppID of the game to check.</param>
    /// <returns>Either the compatibility tier or "unknown"</returns>
    public async Task<string> CheckProtonCompatibilityAsync(int appId, bool debug = false)
    {
        // ProtonDB template url
        string url = $"https://www.protondb.com/api/v1/reports/summaries/{appId}.json";

        try
        {
            // get request to protondb api
            if (debug) Console.WriteLine($"Checking Proton compatibility for AppID: {appId}...");
            var response = await _client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return "unknown";
            }
            // if it works, pull the tier and return it as a string
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            var protonData = JsonSerializer.Deserialize<ProtonDbResponse>(jsonString);
            if (debug) Console.WriteLine($"Proton compatibility for AppID: {appId} is {protonData?.Tier.ToLower() ?? "unknown"}");
            if (protonData?.Tier == "borked") protonData.Tier = "Incompatible";
            return protonData?.Tier.ToLower() ?? "unknown";
        }
        catch
        {
            return "error";
        }
    }
}
// steam api response models
// protondb api response model; all we need is the tier.
public interface IClientService
{
    Task<List<SteamGame>> GetSteamLibraryAsync(string apiKey, string steamId);
    Task<string> CheckProtonCompatibilityAsync(int appId, bool debug = false);
}