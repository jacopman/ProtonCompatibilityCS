using Moq;
using APIServices;
using JsonResponseModels;
using ProtonCompatibility;

namespace Testing;

public class GameFilterTests
{
    Mock<IClientService> _mockService;
    List<SteamGame> _testLibrary;

    public GameFilterTests()
    {
        _mockService = new Mock<IClientService>();
        _testLibrary = new List<SteamGame>
        {
            new SteamGame { AppId = 455, Name = "Gold Game" },
            new SteamGame { AppId = 2, Name = "Bronze Game" },
            new SteamGame { AppId = 3, Name = "Another Gold Game" }
        };
    }

    [Fact]
    public async Task FilterGamesByTier_ShouldOnlyReturnGames_MatchingTheSelectedTier()
    {
        // 1. Arrange: Setup Mock data
        var mockService = _mockService;
        var testLibrary = _testLibrary;

        // Mock the compatibility API responses
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(455)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(2)).ReturnsAsync("bronze");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(3)).ReturnsAsync("gold");

        // 2. Act: Run the filter logic using the mocked service
        var result = await Program.FilterGamesByTier(mockService.Object, testLibrary, "gold");
        // 
        // 3. Assert: Verify only Gold games were compiled
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "Gold Game");
        Assert.Contains(result, g => g.Name == "Another Gold Game");
        Assert.DoesNotContain(result, g => g.Name == "Bronze Game");
    }
    [Fact]
    public async Task FilterGamesByTier_ShouldReturnEmpty_WhenLibraryIsEmpty()
    {
        // Given
        var mockService = _mockService;
        var testLibrary = _testLibrary;
        testLibrary.RemoveAll(g => true);
        // When
        var result = await Program.FilterGamesByTier(mockService.Object, testLibrary, "gold");

        // Then
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterGamesByTier_ShouldBeCaseInsensitive_WhenComparingTiers()
    {
        // Given
        var mockService = _mockService;
        var testLibrary = _testLibrary;

        mockService.Setup(s => s.CheckProtonCompatibilityAsync(455)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(2)).ReturnsAsync("bronze");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(3)).ReturnsAsync("gold");
        // When
        Task<List<(string Name, int AppId)>> resultOne = Program.FilterGamesByTier(mockService.Object, testLibrary, "gold");
        Task<List<(string Name, int AppId)>> resultTwo = Program.FilterGamesByTier(mockService.Object, testLibrary, "Gold");
        Task<List<(string Name, int AppId)>> resultThree = Program.FilterGamesByTier(mockService.Object, testLibrary, "GOLD");

        await Task.WhenAll(resultOne, resultTwo, resultThree);
        // Then

        // Result 1
        Assert.Equal(2, resultOne.Result.Count);
        Assert.Contains(resultOne.Result, g => g.Name == "Gold Game");
        Assert.Contains(resultOne.Result, g => g.Name == "Another Gold Game");
        Assert.DoesNotContain(resultOne.Result, g => g.Name == "Bronze Game");

        // Result 2
        Assert.Equal(2, resultTwo.Result.Count);
        Assert.Contains(resultTwo.Result, g => g.Name == "Gold Game");
        Assert.Contains(resultTwo.Result, g => g.Name == "Another Gold Game");
        Assert.DoesNotContain(resultTwo.Result, g => g.Name == "Bronze Game");
        
        // Result 3
        Assert.Equal(2, resultThree.Result.Count);
        Assert.Contains(resultThree.Result, g => g.Name == "Gold Game");
        Assert.Contains(resultThree.Result, g => g.Name == "Another Gold Game");
        Assert.DoesNotContain(resultThree.Result, g => g.Name == "Bronze Game");
    }

    [Fact]
    public async Task FilterGamesByTier_ShouldHandleRateLimitingDelay()
    {
        _testLibrary.RemoveAll(g => true);
        var mockService = _mockService;
        var testLibrary = _testLibrary;
        _testLibrary.AddRange(new[]
        {
            new SteamGame { AppId = 455, Name = "Gold Game" },
            new SteamGame { AppId = 2, Name = "Bronze Game" },
            new SteamGame { AppId = 25, Name = "Platinum Game" },
            new SteamGame { AppId = 22, Name = "silver Game" },
            new SteamGame { AppId = 39, Name = "Another Platinum Game" },
            new SteamGame { AppId = 3446, Name = "Another Gold Game" },
            new SteamGame { AppId = 372, Name = "One more Gold Game" },
            new SteamGame { AppId = 123, Name = "Another Bronze Game" },
            new SteamGame { AppId = 853, Name = "One more platinum Game" },
            new SteamGame { AppId = 31, Name = "Final Gold Game" },
            new SteamGame { AppId = 38, Name = "another Silver Gold Game" }
        });

        mockService.Setup(s => s.CheckProtonCompatibilityAsync(455)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(2)).ReturnsAsync("bronze");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(25)).ReturnsAsync("platinum");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(22)).ReturnsAsync("silver");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(39)).ReturnsAsync("platinum");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(3446)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(372)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(123)).ReturnsAsync("bronze");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(853)).ReturnsAsync("platinum");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(31)).ReturnsAsync("gold");
        mockService.Setup(s => s.CheckProtonCompatibilityAsync(38)).ReturnsAsync("silver");

        var exception = await Record.ExceptionAsync(() =>
        {
            return Program.FilterGamesByTier(mockService.Object, testLibrary, "gold");
        });
        Assert.Null(exception);
    }
}
