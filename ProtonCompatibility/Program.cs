using System.Text.Json;
using System.Text.Json.Serialization;
using dotenv.net;

class Program
{
    // ___ CONFIGURATION ___
    private static readonly string SteamApiKey = Environment.GetEnvironmentVariable("SteamApiKey");
    private static readonly string SteamId = Environment.GetEnvironmentVariable("SteamId");
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

        Console.WriteLine("Fetching your Steam library...");
        var library = await GetSteamLibraryAsync(SteamApiKey, SteamId);
        if (library == null || library.Count == 0)
        {
            Console.WriteLine("No games found. Check your Steam privacy settings (Game Details must be Public).");
            return;
        }
        Console.WriteLine($"Found {library.Count} games. Checking ProtonDB compatibility...");
        List<(string Name, int AppId)> incompatibleGames = new List<(string Name, int AppId)>();
        int count = 0;
        foreach (var game in library)
        {
            string tier = await CheckProtonCompatilityAsync(game.AppId);

            if (tier == "borked")
            {
                incompatibleGames.Add((game.Name, game.AppId));
                Console.WriteLine($"{game.Name} is BORKED.");
            }

            // Rate limiting safety: Pase briefly every 10 requests
            count++;
            if (count % 10 == 0)
            {
                await Task.Delay(1000);
            }
        }
        // --- RESULTS ---
        Console.WriteLine("\n" + new string('=', 40));
        Console.WriteLine("INCOMPATIBLE GAMES LIST ");
        Console.WriteLine(new string('=', 40));

        if (incompatibleGames.Count > 0)
        {
            foreach (var game in incompatibleGames)
            {
                Console.WriteLine($"- {game.Name} (AppID: {game.AppId}) -> Borked");
            }
        }
        else
        {
            Console.WriteLine("Awesome! None of your owned games are flagged as 'Borked' on ProtonDB.");
        }
    }
    private static async Task<List<SteamGame>> GetSteamLibraryAsync(string apiKey, string steamId)
    {
        string url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={apiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=true&format=json";

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            var steamData = JsonSerializer.Deserialize<SteamApiResponse>(jsonString);
            return steamData?.Response?.Games ?? new List<SteamGame>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Steam library: {ex.Message}");
            return null;
        }
    }
    private static async Task<string> CheckProtonCompatilityAsync(int appId)
    {
        string url = $"https://www.protondb.com/api/v1/reports/summaries/{appId}.json";

        try
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return "native/unknown";
            }
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            var protonData = JsonSerializer.Deserialize<ProtonDbResponse>(jsonString);
            return protonData?.Tier.ToLower() ?? "unknown";
        }
        catch
        {
            return "error";
        }
    }
}
internal class SteamApiResponse
{
    [JsonPropertyName("response")]
    public required SteamResponseData Response { get; set; }
}
internal class SteamResponseData
{
    [JsonPropertyName("game_count")]
    public int GameCount { get; set; }

    [JsonPropertyName("games")]
    public required List<SteamGame> Games { get; set; }
}
internal class SteamGame
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
internal class ProtonDbResponse
{
    [JsonPropertyName("tier")]
    public required string Tier { get; set; }
}