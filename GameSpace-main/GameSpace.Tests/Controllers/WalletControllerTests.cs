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
/// WalletController 整合測試
/// </summary>
public class WalletControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly GameSpaceDbContext _context;

    public WalletControllerTests(WebApplicationFactory<Program> factory)
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
    public async Task GetWallet_當錢包存在時_應該返回錢包資訊()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/wallet/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var walletData = JsonSerializer.Deserialize<UserWallet>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        walletData.Should().NotBeNull();
        walletData.User_Id.Should().Be(userId);
        walletData.User_Point.Should().Be(1000);
        walletData.User_Coin.Should().Be(100);
    }

    [Fact]
    public async Task GetWallet_當錢包不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;

        // Act
        var response = await _client.GetAsync($"/api/wallet/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateWallet_應該成功創建錢包()
    {
        // Arrange
        var userId = 1;

        // Act
        var response = await _client.PostAsync($"/api/wallet/{userId}", null);

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
    public async Task CreateWallet_當錢包已存在時_應該返回現有錢包()
    {
        // Arrange
        var userId = 1;
        var existingWallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(existingWallet);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/wallet/{userId}", null);

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
    public async Task AddPoints_當錢包存在時_應該成功增加點數()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var pointsToAdd = 500;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/add-points", new { points = pointsToAdd });

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
    public async Task AddPoints_當錢包不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var pointsToAdd = 500;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/add-points", new { points = pointsToAdd });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeductPoints_當點數足夠時_應該成功扣除點數()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var pointsToDeduct = 300;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-points", new { points = pointsToDeduct });

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
    public async Task DeductPoints_當點數不足時_應該返回400()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 100,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var pointsToDeduct = 500;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-points", new { points = pointsToDeduct });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeductPoints_當錢包不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var pointsToDeduct = 300;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-points", new { points = pointsToDeduct });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddCoins_當錢包存在時_應該成功增加金幣()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var coinsToAdd = 50;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/add-coins", new { coins = coinsToAdd });

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
    public async Task AddCoins_當錢包不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var coinsToAdd = 50;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/add-coins", new { coins = coinsToAdd });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeductCoins_當金幣足夠時_應該成功扣除金幣()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var coinsToDeduct = 30;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-coins", new { coins = coinsToDeduct });

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
    public async Task DeductCoins_當金幣不足時_應該返回400()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 10
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var coinsToDeduct = 50;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-coins", new { coins = coinsToDeduct });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeductCoins_當錢包不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var coinsToDeduct = 30;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/wallet/{userId}/deduct-coins", new { coins = coinsToDeduct });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactionHistory_應該返回交易歷史()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Transaction_ID = 1,
                User_ID = userId,
                Transaction_Type = "Point_Earn",
                Transaction_Amount = 500,
                Transaction_Description = "遊戲獎勵",
                Transaction_Time = DateTime.UtcNow.AddHours(-2)
            },
            new Transaction
            {
                Transaction_ID = 2,
                User_ID = userId,
                Transaction_Type = "Coin_Spend",
                Transaction_Amount = -50,
                Transaction_Description = "購買道具",
                Transaction_Time = DateTime.UtcNow.AddHours(-1)
            }
        };

        _context.UserWallets.Add(wallet);
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/wallet/{userId}/transactions?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var transactionList = JsonSerializer.Deserialize<List<Transaction>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        transactionList.Should().HaveCount(2);
        transactionList.Should().BeInDescendingOrder(t => t.Transaction_Time);
    }

    [Fact]
    public async Task GetTransactionHistory_當沒有交易記錄時_應該返回空列表()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/wallet/{userId}/transactions?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var transactionList = JsonSerializer.Deserialize<List<Transaction>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        transactionList.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionHistory_當限制數量時_應該只返回指定數量的記錄()
    {
        // Arrange
        var userId = 1;
        var wallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000,
            User_Coin = 100
        };

        var transactions = new List<Transaction>();
        for (int i = 1; i <= 15; i++)
        {
            transactions.Add(new Transaction
            {
                Transaction_ID = i,
                User_ID = userId,
                Transaction_Type = "Point_Earn",
                Transaction_Amount = 100,
                Transaction_Description = $"交易 {i}",
                Transaction_Time = DateTime.UtcNow.AddHours(-i)
            });
        }

        _context.UserWallets.Add(wallet);
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/wallet/{userId}/transactions?limit=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var transactionList = JsonSerializer.Deserialize<List<Transaction>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        transactionList.Should().HaveCount(10);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}