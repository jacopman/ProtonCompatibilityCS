using dotenv.net;
using JsonResponseModels;
using APIServices;

namespace ProtonCompatibility;

public class Program
{
    // ___ CONFIGURATION ___
    private static readonly string? SteamApiKey = Environment.GetEnvironmentVariable("SteamApiKey");
    private static readonly string? SteamId = Environment.GetEnvironmentVariable("SteamId");
    private static readonly ISteamService _steamService = new SteamService(new HttpClient());

    static async Task Main(string[] args)
    {
        if (string.IsNullOrEmpty(SteamApiKey) || string.IsNullOrEmpty(SteamId))
        {
            Console.WriteLine("Error: Missing Steam API key or Steam ID. Please set the environment variables 'SteamApiKey' and 'SteamId'.");
            return;
        }
        else
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Console.Clear();

            Console.WriteLine("Fetching your Steam library...");
            var library = await _steamService.GetSteamLibraryAsync(SteamApiKey, SteamId);
            if (library == null || library.Count == 0)
            {
                Console.WriteLine("No games found. Check your Steam privacy settings (Game Details must be Public).");
                return;
            }
            Console.WriteLine($"Found {library.Count} games. please select a compatibility tier to filter by...");
            // selecting a tier to filter games by
            string selected = FilterSelection();


            // --- FILTER GAMES BY SELECTED TIER ---
            List<(string Name, int AppId)> filteredGamesList = await FilterGamesByTier(_steamService, library, selected);
            Console.WriteLine($"Filtering games by {selected.ToUpper()} tier... Found {filteredGamesList.Count} matching games.\n");
            // --- RESULTS ---
            DisplayCompatibility(filteredGamesList, selected);
        }
    }
    /// <summary>
    /// Displays a console menu for selecting a compatibility tier and returns the selected tier as a string.
    /// </summary>
    /// <returns>the selected tier to normalize function inputs</returns>
    private static string FilterSelection()
    {
        string[] searchType = new string[] { "Incompatible", "Bronze", "Silver", "Gold", "Platinum", "Compatible" };
        bool selected = false;
        int selectedOption = 0;
        string prompt = "Select a compatibility tier to search for (⌃ + ⌄ to navigate, Enter to select):";
        while (!selected)
        {
            Console.Clear();
            Console.WriteLine(prompt);
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
                prompt = "Select a compatibility tier to search for (⌃ + ⌄ to navigate, Enter to select):";

            }
            else if (selectedKey == "DownArrow")
            {
                selectedOption = selectedOption == searchType.Length - 1 ? 0 : selectedOption + 1;
                prompt = "Select a compatibility tier to search for (⌃ + ⌄ to navigate, Enter to select):";
            }
            else if (selectedKey == "Enter")
            {
                // Process the selected choice
                selected = true;
                prompt = "Select a compatibility tier to search for (⌃ + ⌄ to navigate, Enter to select):";
                // Console.WriteLine($"You selected: {searchType[selectedOption]}");
            }
            else
            {
                prompt = "Invalid input. Please use the arrow keys to navigate and Enter to select.";
                Task.Delay(1000);
            }
        }
        return searchType[selectedOption];
    }
    /// <summary>
    /// Displays a list of games filtered by the specified compatibility tier, along with their AppIDs and a total count of games found for that tier.
    /// </summary>
    /// <param name="games">The list of games to display.</param>
    /// <param name="tier">The compatibility tier to filter by.</param>
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
    /// <summary>
    /// Filters a list of Steam games based on their compatibility tier by checking each game's AppID against the ProtonDB API and comparing the returned tier with the specified tier. The method returns a list of tuples containing the name and AppID of each game that matches the specified tier.
    /// </summary>
    /// <param name="steamService">An instance of the ISteamService interface used to check game compatibility.</param>
    /// <param name="library">The list of Steam games to filter.</param>
    /// <param name="tier">The compatibility tier to filter by.</param>
    /// <returns>A list of tuples containing the name and AppID of each game that matches the specified tier.</returns>
    public static async Task<List<(string Name, int AppId)>> FilterGamesByTier(ISteamService steamService, List<SteamGame> library, string tier)
    {
        List<(string Name, int AppId)> filteredGames = new List<(string Name, int AppId)>();
        int count = 0;
        foreach (var game in library)
        {
            string gameTier = await steamService.CheckProtonCompatibilityAsync(game.AppId);
            if (gameTier == tier.ToLower())
            {
                filteredGames.Add((game.Name, game.AppId));
            }
            // Rate limiting safety: Pause briefly every 10 requests
            count++;
            if (count % 10 == 0)
            {
                await Task.Delay(1000);
            }


        }
        return filteredGames;
    }
}