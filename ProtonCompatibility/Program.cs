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

        string[] searchType = new string[] { "Incompatible", "Bronze", "Silver", "Gold", "Platinum", "Compatible" };
        bool selected = false;
        int selectedOption = 0;
        // --- INTERACTIVE TIER SELECTION ---
        while (!selected)
        {
            Console.Clear();
            Console.WriteLine("Select a compatibility tier to search for (⌃ + ⌄ to navigate, Enter to select):");
            // display search options
            for (int i = 0; i < searchType.Length; i++)
            {
                if (i == selectedOption)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine($"{i + 1}. {searchType[i]}");
                Console.ResetColor();
            }
            Console.Write("");
            // options navigation
            string selectedKey = Console.ReadKey(true).Key.ToString();
            if (selectedKey == "UpArrow")
            {
                selectedOption = selectedOption == 0 ? searchType.Length - 1 : selectedOption - 1;
            }
            else if (selectedKey == "DownArrow")
            {
                selectedOption = selectedOption == searchType.Length - 1 ? 0 : selectedOption + 1;
            }
            else if (selectedKey == "Enter")
            {
                // Process the selected choice
                selected = true;
                // Console.WriteLine($"You selected: {searchType[selectedOption]}");
                await Task.Delay(500);
            }
            else
            {
                Console.WriteLine("Invalid input. Please use the arrow keys to navigate and Enter to select.");
                await Task.Delay(1000);
            }
        }

        Console.WriteLine("Fetching your Steam library...");
        var library = await GetSteamLibraryAsync(SteamApiKey, SteamId);
        if (library == null || library.Count == 0)
        {
            Console.WriteLine("No games found. Check your Steam privacy settings (Game Details must be Public).");
            return;
        }
        Console.WriteLine($"Found {library.Count} games. Checking ProtonDB compatibility...");

        // --- FILTER GAMES BY SELECTED TIER ---
        List<(string Name, int AppId)> filteredGamesList = await FilterGamesByTier(library, searchType[selectedOption]);

        // --- RESULTS ---
        DisplayCompatibility(filteredGamesList, searchType[selectedOption]);

    }
    private static async Task<List<(string Name, int AppId)>> FilterGamesByTier(List<SteamGame> library, string tier)
    {
        List<(string Name, int AppId)> filteredGames = new List<(string Name, int AppId)>();
        int count = 0;
        foreach (var game in library)
        {
            string gameTier = await CheckProtonCompatilityAsync(game.AppId, tier);
            if (gameTier == tier.ToLower())
            {
                filteredGames.Add((game.Name, game.AppId));
            }
            // Rate limiting safety: Pase briefly every 10 requests
            count++;
            if (count % 10 == 0)
            {
                await Task.Delay(1000);
            }


        }
        return filteredGames;
    }
    private static void DisplayCompatibility(List<(string Name, int AppId)> games, string tier)
    {
        Console.WriteLine("\n" + new string('=', 40));
        Console.WriteLine($"{tier.ToUpper()} GAMES LIST ");
        Console.WriteLine(new string('=', 40));

        if (games.Count > 0)
        {
            // foreach (var game in games)
            // {
            //     Console.WriteLine($"- {game.Name} (AppID: {game.AppId})\n");
            // }
            for (int i = 1; i <= games.Count; i++)
            {
                Console.WriteLine($"{i} - {games[i - 1].Name} (AppID: {games[i - 1].AppId})");
            }
            Console.WriteLine($"\nTotal: {games.Count} game(s) of {tier.ToUpper()} tier found.\n");
        }
        else
        {
            Console.WriteLine($"no games of {tier.ToUpper()} tier found.\n");
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
    private static async Task<string> CheckProtonCompatilityAsync(int appId, string tier)
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