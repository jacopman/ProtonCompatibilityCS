using APIServices;
using JsonResponseModels;

namespace ProtonCompatibility.Utilities;
/// <summary>
/// Provides utility methods for interacting with the ProtonDB API and filtering Steam games based on their compatibility tier. This class is designed
/// </summary>
public static class Wrappers
{
    /// <summary>
    /// Provides a static instance of the IClientService interface, allowing for interaction with the ProtonDB API and filtering of Steam games based on their compatibility tier.
    /// </summary>
    public static readonly IClientService _clientService = new ClientService(new HttpClient());

    /// <summary>
    /// The main entry point of the application, which fetches the user's Steam library, filters the games based on a selected compatibility tier, and displays the results in the console. The method takes an API key and a Steam ID as parameters, which are used to authenticate with the ProtonDB API and retrieve the user's library. If either parameter is missing, an error message is displayed and the method returns early. The user is prompted to select a compatibility tier to filter by, and the filtered results are displayed in the console along with a total count of matching games.
    /// </summary>
    /// <param name="apiKey">The API key for authenticating with the ProtonDB API.</param>
    /// <param name="steamId">The Steam ID of the user whose library is to be retrieved.</param>
    /// <returns></returns>
    public static async Task Main(string? apiKey, string? steamId)
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(steamId))
        {
            Console.WriteLine("Error: Missing API key or Steam ID. Please set the environment variables 'ApiKey' and 'SteamId'.");
            return;
        }
        Console.Clear();

        // selecting a tier to filter games by
        string filterMode = FilterMode();

        Console.Write("Fetching your Steam library...");
        var library = await _clientService.GetSteamLibraryAsync(apiKey, steamId);
        try
        {
            Func.ValidateLibrary(library);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return;
        }
        Task<List<(string Name, int AppId)>> filteredGamesTask = Func.FilterGamesByTier(_clientService, library, filterMode);
        Console.Write($"Found {library.Count} games. ");

        // --- FILTER GAMES BY SELECTED TIER ---
        Console.Write($"Filtering games by {filterMode.ToUpper()} tier...");
        Console.WriteLine($" Found {filteredGamesTask.Result.Count} matching games.\n");
        // --- RESULTS ---
        DisplayCompatibility(filteredGamesTask.Result, filterMode);

    }
    /// <summary>
    /// Displays a console menu for selecting a compatibility tier and returns the selected tier as a string.
    /// </summary>
    /// <returns>the selected tier to normalize function inputs</returns>

    static string FilterMode()
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


}
/// <summary>
/// Provides utility methods for filtering and validating Steam games based on their compatibility tier, specifically designed for use in a Blazor application. This class is needed because the main Wrappers class uses a Main() method which limits its use cases.
/// </summary>
internal static class Func
{
    /// <summary>
    /// Filters a list of Steam games based on their compatibility tier by checking each game's AppID against the ProtonDB API and comparing the returned tier with the specified tier. The method returns a list of tuples containing the name and AppID of each game that matches the specified tier.
    /// </summary>
    /// <param name="clientService">An instance of the IClientService interface used to check game compatibility.</param>
    /// <param name="library">The list of Steam games to filter.</param>
    /// <param name="filterTier">The compatibility tier to filter by.</param>
    /// <returns>A list of tuples containing the name and AppID of each game that matches the specified tier.</returns>
    public static async Task<List<(string Name, int AppId)>> FilterGamesByTier(IClientService clientService, List<SteamGame> library, string filterTier)
    {
        List<(string Name, int AppId)> filteredGames = [];
        int count = 0;
        foreach (var game in library)
        {
            string gameTier = await clientService.CheckProtonCompatibilityAsync(game.AppId);
            if (gameTier.Equals(filterTier, StringComparison.CurrentCultureIgnoreCase))
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
/// <summary>
/// Validates that the provided list of Steam games is not null or empty. If the list is null or empty, the method throws an ArgumentNullException with a descriptive message.
/// </summary>
/// <param name="library">The list of Steam games to validate.</param>
/// <exception cref="ArgumentNullException">Thrown when the library is null or empty.</exception>
    public static void ValidateLibrary(List<SteamGame> library)
    {
        if (library == null || library.Count == 0)
        {
            throw new ArgumentNullException($"{nameof(library)} cannot be null or empty.");
        }
    }
}
/// <summary>
/// Provides utility methods for interacting with the ProtonDB API and filtering Steam games based on their compatibility tier, specifically designed for use in a Blazor application.
/// this is needed because the main Wrappers class uses a Main() method which limits its use cases.
/// </summary>
public static class BlazorWrappers
{
    /// <summary>
    /// Provides a static instance of the IClientService interface, allowing for interaction with the ProtonDB API and filtering of Steam games based on their compatibility tier.
    /// </summary>
    public static readonly IClientService _clientService = new ClientService(new HttpClient());
    
    /// <summary>
    /// Filters a list of Steam games based on their compatibility tier by checking each game's AppID against the ProtonDB API and comparing the returned tier with the specified tier. The method returns a list of tuples containing the name, AppID, and compatibility tier of each game that matches the specified tier.
    /// </summary>
    /// <param name="library">The list of Steam games to filter.</param>
    /// <returns>A list of tuples containing the name, AppID, and compatibility tier of each game that matches the specified tier.</returns>
    public static async Task<List<(string Name, int AppId, string Tier)>> GetGameCompatibility(List<SteamGame> library)
    {
        List<(string Name, int AppId, string Tier)> compatibilityList = [];
        foreach (SteamGame game in library)
        {
            Console.WriteLine($"Checking compatibility for {game.Name} (AppID: {game.AppId})...");
            string tier = await _clientService.CheckProtonCompatibilityAsync(game.AppId);
            compatibilityList.Add((game.Name, game.AppId, tier));
        }
        return compatibilityList;
    }
    // public static async Task<List<SteamGame>> GetLibrary(string? apiKey, string? steamId)
    // {
    //     if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(steamId))
    //     {
    //         Console.WriteLine("Error: Missing API key or Steam ID. Please set the environment variables 'ApiKey' and 'SteamId'.");
    //         return [];
    //     }
    //     var library = await _clientService.GetSteamLibraryAsync(apiKey, steamId);
    //     return library ?? [];
    // }
    /// <summary>
    /// Filters a list of games based on their compatibility tier, returning only those that match the specified tier. The method takes a list of tuples containing the name, AppID, and compatibility tier of each game, and returns a new list containing only those games that match the specified tier.
    /// </summary>
    /// <param name="compatibilityList">The list of games to filter.</param>
    /// <param name="filterTier">The compatibility tier to filter by.</param>
    /// <returns>A list of tuples containing the name, AppID, and compatibility tier of each game that matches the specified tier.</returns>
    public static List<(string Name, int AppId, string Tier)> FilterGames(List<(string Name, int AppId, string Tier)> compatibilityList, string filterTier)
    {
        List<(string Name, int AppId, string Tier)> filteredGames = [];
        if (filterTier.Equals("Compatible", StringComparison.CurrentCultureIgnoreCase))
        {
            foreach (var game in compatibilityList)
            {
                if (!game.Tier.Equals("borked", StringComparison.CurrentCultureIgnoreCase))
                {
                    filteredGames.Add(game);
                }
            }
        }
        else if (filterTier.Equals("Incompatible", StringComparison.CurrentCultureIgnoreCase))
        {
            foreach (var game in compatibilityList)
            {
                if (game.Tier.Equals("borked", StringComparison.CurrentCultureIgnoreCase))
                {
                    filteredGames.Add(game);
                }
            }

        }
        else
        {
            foreach (var game in compatibilityList)
            {
                if (game.Tier.Equals(filterTier, StringComparison.CurrentCultureIgnoreCase))
                {
                    filteredGames.Add(game);
                }
            }
        }
        return filteredGames;
    }

}