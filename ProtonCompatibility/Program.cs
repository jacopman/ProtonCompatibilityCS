using dotenv.net;
using ProtonCompatibility.Utilities;

namespace ProtonCompatibility;

public class Program
{
    // ___ CONFIGURATION ___
    private static readonly string? SteamApiKey = Environment.GetEnvironmentVariable("SteamApiKey");
    private static readonly string? SteamId = Environment.GetEnvironmentVariable("SteamId");



    static async Task Main(string[] args)
    {

        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

        try
        {
            await Wrappers.Main(SteamApiKey, SteamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }


}