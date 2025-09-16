using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json;

namespace GameSpace.Tests.Controllers;

/// <summary>
/// GameController 整合測試
/// </summary>
public class GameControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly GameSpaceDbContext _context;

    public GameControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 移除現有的 DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameSpaceDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 添加 In-Memory 資料庫
                services.AddDbContext<GameSpaceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
        _context = _factory.Services.GetRequiredService<GameSpaceDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetGames_應該返回所有遊戲()
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
        var response = await _client.GetAsync("/api/game");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameList = JsonSerializer.Deserialize<List<Game>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        gameList.Should().HaveCount(2);
        gameList.Should().Contain(g => g.Game_Name == "遊戲1");
        gameList.Should().Contain(g => g.Game_Name == "遊戲2");
    }

    [Fact]
    public async Task GetGame_當遊戲存在時_應該返回遊戲資訊()
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
        var response = await _client.GetAsync($"/api/game/{gameId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameData = JsonSerializer.Deserialize<Game>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        gameData.Should().NotBeNull();
        gameData.Game_ID.Should().Be(gameId);
        gameData.Game_Name.Should().Be("測試遊戲");
    }

    [Fact]
    public async Task GetGame_當遊戲不存在時_應該返回404()
    {
        // Arrange
        var gameId = 999;

        // Act
        var response = await _client.GetAsync($"/api/game/{gameId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateGame_應該成功創建遊戲()
    {
        // Arrange
        var gameData = new
        {
            Game_Name = "新遊戲",
            Game_Description = "新遊戲描述"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/game", gameData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task UpdateGame_當遊戲存在時_應該成功更新遊戲()
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
        var response = await _client.PutAsJsonAsync($"/api/game/{gameId}", updateData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task UpdateGame_當遊戲不存在時_應該返回404()
    {
        // Arrange
        var gameId = 999;
        var updateData = new
        {
            Game_Name = "更新遊戲",
            Game_Description = "更新描述"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/game/{gameId}", updateData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteGame_當遊戲存在時_應該成功刪除遊戲()
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
        var response = await _client.DeleteAsync($"/api/game/{gameId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task DeleteGame_當遊戲不存在時_應該返回404()
    {
        // Arrange
        var gameId = 999;

        // Act
        var response = await _client.DeleteAsync($"/api/game/{gameId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGameRankings_應該返回遊戲排行榜()
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
        var response = await _client.GetAsync($"/api/game/{gameId}/rankings?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var rankings = JsonSerializer.Deserialize<List<GameRecord>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        rankings.Should().HaveCount(3);
        rankings.First().Game_Score.Should().Be(2000); // 最高分
        rankings.Last().Game_Score.Should().Be(1000); // 最低分
    }

    [Fact]
    public async Task GetGameRankings_當遊戲沒有記錄時_應該返回空列表()
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
        var response = await _client.GetAsync($"/api/game/{gameId}/rankings?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var rankings = JsonSerializer.Deserialize<List<GameRecord>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        rankings.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGameRankings_當限制數量時_應該只返回指定數量的記錄()
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
        var response = await _client.GetAsync($"/api/game/{gameId}/rankings?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var rankings = JsonSerializer.Deserialize<List<GameRecord>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        rankings.Should().HaveCount(10);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}