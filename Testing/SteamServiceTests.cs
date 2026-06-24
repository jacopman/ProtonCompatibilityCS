using Moq;
using APIServices;
using JsonResponseModels;

namespace Testing;

public class SteamService
{
    public SteamService()
    {
        _mockService = new Mock<IClientService>();
        _testLibrary = [];
    }

    Mock<IClientService> _mockService;
    List<SteamGame> _testLibrary;

    [Fact]
    public async Task GetSteamLibraryAsync_ShouldReturnOneGame_WhenApiReturns200OK_andThereIsOneGame()
    {
        // given
        var mockService = _mockService;
        List<SteamGame> testLibrary = _testLibrary;
        mockService
            .Setup(x => x.GetSteamLibraryAsync(apiKey: "14", steamId: "223"))
            .ReturnsAsync(new List<SteamGame> { new SteamGame {AppId = 14, Name = "Game Name"}});
        var service = mockService.Object;

        // when
        testLibrary = await service.GetSteamLibraryAsync(apiKey: "14", steamId: "223");
        // Then

        Assert.Contains("Game Name", testLibrary.Select(g => g.Name));
    }
    [Fact]
    public async Task GetSteamLibraryAsync_ShouldReturnGames_WhenApiReturns200OK_andALibraryExists()
    {
        // given
        var mockService = _mockService;
        List<SteamGame> testLibrary = _testLibrary;
        mockService
            .Setup(x => x.GetSteamLibraryAsync(apiKey: "14", steamId: "223"))
            .ReturnsAsync(new List<SteamGame> { 
                new SteamGame {AppId = 14, Name = "Gold Game"},
                new SteamGame {AppId = 15, Name = "Silver Game"},
                new SteamGame {AppId = 16, Name = "Bronze Game"}
                });
        var service = mockService.Object;

        // when
        testLibrary = await service.GetSteamLibraryAsync(apiKey: "14", steamId: "223");
        // Then

        Assert.Contains("Gold Game", testLibrary.Select(g => g.Name));
        Assert.Contains("Silver Game", testLibrary.Select(g => g.Name));
        Assert.Contains("Bronze Game", testLibrary.Select(g => g.Name));
    }
    [Fact]
    public async Task GetSteamLibraryAsync_ShouldNotReturnGames_whenLibraryDoesNotContainThem()
    {
        // given
        var mockService = _mockService;
        List<SteamGame> testLibrary = _testLibrary;
            // setting up the mock
        mockService
            .Setup(x => x.GetSteamLibraryAsync(apiKey: "14", steamId: "223"))
            .ReturnsAsync(new List<SteamGame> { 
                new SteamGame {AppId = 14, Name = "Gold Game"},
                new SteamGame {AppId = 15, Name = "Silver Game"},
                new SteamGame {AppId = 16, Name = "Bronze Game"}
                });
            // applying the apiReturn to a callable object
        var service = mockService.Object;

        // when
        testLibrary = await service.GetSteamLibraryAsync(apiKey: "14", steamId: "223");

        // Then
        Assert.Contains("Gold Game", testLibrary.Select(g => g.Name));
        Assert.Contains("Silver Game", testLibrary.Select(g => g.Name));
        Assert.Contains("Bronze Game", testLibrary.Select(g => g.Name));
        Assert.DoesNotContain("Platinum Game", testLibrary.Select(g => g.Name));
    }

    [Fact]
    public async Task GetSteamLibraryAsync_ShouldNotReturnGames_WhenAPIReturns404()
    {
        // given
        var mockService = _mockService;
        List<SteamGame> testLibrary = _testLibrary;
            // setting up the mock
        mockService
            .Setup(x => x.GetSteamLibraryAsync(apiKey: "14", steamId: "223"))
            .ReturnsAsync(new List<SteamGame>());
            // applying the apiReturn to a callable object
        var service = mockService.Object;

        // when
        testLibrary = await service.GetSteamLibraryAsync(apiKey: "14", steamId: "223");

        // Then
        Assert.Empty(testLibrary);
    }

}