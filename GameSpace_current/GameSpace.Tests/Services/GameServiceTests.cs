using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;

namespace GameSpace.Tests.Services;

/// <summary>
/// GameService 單元測試
/// </summary>
public class GameServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly GameService _gameService;
    private readonly Fixture _fixture;

    public GameServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _gameService = new GameService(_context);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetGameByIdAsync_當遊戲存在時_應該返回遊戲資訊()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "測試遊戲",
            Game_Description = "測試遊戲描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.GetGameByIdAsync(gameId);

        // Assert
        result.Should().NotBeNull();
        result.Game_ID.Should().Be(gameId);
        result.Game_Name.Should().Be("測試遊戲");
    }

    [Fact]
    public async Task GetGameByIdAsync_當遊戲不存在時_應該返回null()
    {
        // Arrange
        var gameId = 999;

        // Act
        var result = await _gameService.GetGameByIdAsync(gameId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllGamesAsync_應該返回所有遊戲()
    {
        // Arrange
        var games = new List<Game>
        {
            new Game
            {
                Game_ID = 1,
                Game_Name = "遊戲1",
                Game_Description = "遊戲1描述",
                Game_Status = "Active",
                Game_Create_Time = DateTime.UtcNow,
                Game_Update_Time = DateTime.UtcNow
            },
            new Game
            {
                Game_ID = 2,
                Game_Name = "遊戲2",
                Game_Description = "遊戲2描述",
                Game_Status = "Active",
                Game_Create_Time = DateTime.UtcNow,
                Game_Update_Time = DateTime.UtcNow
            }
        };

        _context.Games.AddRange(games);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.GetAllGamesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(g => g.Game_Name == "遊戲1");
        result.Should().Contain(g => g.Game_Name == "遊戲2");
    }

    [Fact]
    public async Task CreateGameAsync_應該成功創建遊戲()
    {
        // Arrange
        var gameName = "新遊戲";
        var gameDescription = "新遊戲描述";

        // Act
        var result = await _gameService.CreateGameAsync(gameName, gameDescription);

        // Assert
        result.Should().NotBeNull();
        result.Game_Name.Should().Be(gameName);
        result.Game_Description.Should().Be(gameDescription);
        result.Game_Status.Should().Be("Active");
        result.Game_Create_Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateGameAsync_應該成功更新遊戲資訊()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "原始遊戲",
            Game_Description = "原始描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var updateData = new
        {
            Game_Name = "更新遊戲",
            Game_Description = "更新描述"
        };

        // Act
        var result = await _gameService.UpdateGameAsync(
            gameId,
            updateData.Game_Name,
            updateData.Game_Description
        );

        // Assert
        result.Should().BeTrue();
        
        var updatedGame = await _context.Games.FirstAsync(g => g.Game_ID == gameId);
        updatedGame.Game_Name.Should().Be(updateData.Game_Name);
        updatedGame.Game_Description.Should().Be(updateData.Game_Description);
    }

    [Fact]
    public async Task UpdateGameAsync_當遊戲不存在時_應該返回false()
    {
        // Arrange
        var gameId = 999;

        // Act
        var result = await _gameService.UpdateGameAsync(
            gameId,
            "更新遊戲",
            "更新描述"
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteGameAsync_應該成功刪除遊戲()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "測試遊戲",
            Game_Description = "測試描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.DeleteGameAsync(gameId);

        // Assert
        result.Should().BeTrue();
        
        var deletedGame = await _context.Games.FirstOrDefaultAsync(g => g.Game_ID == gameId);
        deletedGame.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGameAsync_當遊戲不存在時_應該返回false()
    {
        // Arrange
        var gameId = 999;

        // Act
        var result = await _gameService.DeleteGameAsync(gameId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetGameRankingsAsync_應該返回遊戲排行榜()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "測試遊戲",
            Game_Description = "測試描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        var gameRecords = new List<GameRecord>
        {
            new GameRecord
            {
                Game_Record_ID = 1,
                User_ID = 1,
                Game_ID = gameId,
                Game_Score = 1000,
                Game_Time = 60,
                Game_Record_Time = DateTime.UtcNow
            },
            new GameRecord
            {
                Game_Record_ID = 2,
                User_ID = 2,
                Game_ID = gameId,
                Game_Score = 2000,
                Game_Time = 45,
                Game_Record_Time = DateTime.UtcNow
            },
            new GameRecord
            {
                Game_Record_ID = 3,
                User_ID = 3,
                Game_ID = gameId,
                Game_Score = 1500,
                Game_Time = 50,
                Game_Record_Time = DateTime.UtcNow
            }
        };

        _context.Games.Add(game);
        _context.GameRecords.AddRange(gameRecords);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.GetGameRankingsAsync(gameId, 10);

        // Assert
        result.Should().HaveCount(3);
        result.First().Game_Score.Should().Be(2000); // 最高分
        result.Last().Game_Score.Should().Be(1000); // 最低分
    }

    [Fact]
    public async Task GetGameRankingsAsync_當遊戲沒有記錄時_應該返回空列表()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "測試遊戲",
            Game_Description = "測試描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.GetGameRankingsAsync(gameId, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGameRankingsAsync_當限制數量時_應該只返回指定數量的記錄()
    {
        // Arrange
        var gameId = 1;
        var game = new Game
        {
            Game_ID = gameId,
            Game_Name = "測試遊戲",
            Game_Description = "測試描述",
            Game_Status = "Active",
            Game_Create_Time = DateTime.UtcNow,
            Game_Update_Time = DateTime.UtcNow
        };

        var gameRecords = new List<GameRecord>();
        for (int i = 1; i <= 15; i++)
        {
            gameRecords.Add(new GameRecord
            {
                Game_Record_ID = i,
                User_ID = i,
                Game_ID = gameId,
                Game_Score = 1000 + i,
                Game_Time = 60,
                Game_Record_Time = DateTime.UtcNow
            });
        }

        _context.Games.Add(game);
        _context.GameRecords.AddRange(gameRecords);
        await _context.SaveChangesAsync();

        // Act
        var result = await _gameService.GetGameRankingsAsync(gameId, 10);

        // Assert
        result.Should().HaveCount(10);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}