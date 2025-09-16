using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;

namespace GameSpace.Tests.Services;

/// <summary>
/// WalletService 單元測試
/// </summary>
public class WalletServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly WalletService _walletService;
    private readonly Fixture _fixture;

    public WalletServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _walletService = new WalletService(_context);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetUserWalletAsync_當錢包存在時_應該返回錢包資訊()
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
        var result = await _walletService.GetUserWalletAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.User_Id.Should().Be(userId);
        result.User_Point.Should().Be(1000);
        result.User_Coin.Should().Be(100);
    }

    [Fact]
    public async Task GetUserWalletAsync_當錢包不存在時_應該返回null()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _walletService.GetUserWalletAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserWalletAsync_應該成功創建錢包()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _walletService.CreateUserWalletAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.User_Id.Should().Be(userId);
        result.User_Point.Should().Be(0);
        result.User_Coin.Should().Be(0);
    }

    [Fact]
    public async Task CreateUserWalletAsync_當錢包已存在時_應該返回現有錢包()
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
        var result = await _walletService.CreateUserWalletAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.User_Id.Should().Be(userId);
        result.User_Point.Should().Be(1000);
        result.User_Coin.Should().Be(100);
    }

    [Fact]
    public async Task AddPointsAsync_當錢包存在時_應該成功增加點數()
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
        var result = await _walletService.AddPointsAsync(userId, pointsToAdd);

        // Assert
        result.Should().BeTrue();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Point.Should().Be(1500);
    }

    [Fact]
    public async Task AddPointsAsync_當錢包不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;
        var pointsToAdd = 500;

        // Act
        var result = await _walletService.AddPointsAsync(userId, pointsToAdd);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeductPointsAsync_當點數足夠時_應該成功扣除點數()
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
        var result = await _walletService.DeductPointsAsync(userId, pointsToDeduct);

        // Assert
        result.Should().BeTrue();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Point.Should().Be(700);
    }

    [Fact]
    public async Task DeductPointsAsync_當點數不足時_應該返回false()
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
        var result = await _walletService.DeductPointsAsync(userId, pointsToDeduct);

        // Assert
        result.Should().BeFalse();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Point.Should().Be(100); // 點數不變
    }

    [Fact]
    public async Task DeductPointsAsync_當錢包不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;
        var pointsToDeduct = 300;

        // Act
        var result = await _walletService.DeductPointsAsync(userId, pointsToDeduct);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddCoinsAsync_當錢包存在時_應該成功增加金幣()
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
        var result = await _walletService.AddCoinsAsync(userId, coinsToAdd);

        // Assert
        result.Should().BeTrue();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Coin.Should().Be(150);
    }

    [Fact]
    public async Task AddCoinsAsync_當錢包不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;
        var coinsToAdd = 50;

        // Act
        var result = await _walletService.AddCoinsAsync(userId, coinsToAdd);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeductCoinsAsync_當金幣足夠時_應該成功扣除金幣()
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
        var result = await _walletService.DeductCoinsAsync(userId, coinsToDeduct);

        // Assert
        result.Should().BeTrue();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Coin.Should().Be(70);
    }

    [Fact]
    public async Task DeductCoinsAsync_當金幣不足時_應該返回false()
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
        var result = await _walletService.DeductCoinsAsync(userId, coinsToDeduct);

        // Assert
        result.Should().BeFalse();
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Coin.Should().Be(10); // 金幣不變
    }

    [Fact]
    public async Task DeductCoinsAsync_當錢包不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;
        var coinsToDeduct = 30;

        // Act
        var result = await _walletService.DeductCoinsAsync(userId, coinsToDeduct);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_應該返回交易歷史()
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
        var result = await _walletService.GetTransactionHistoryAsync(userId, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(t => t.Transaction_Time);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_當沒有交易記錄時_應該返回空列表()
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
        var result = await _walletService.GetTransactionHistoryAsync(userId, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_當限制數量時_應該只返回指定數量的記錄()
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
        var result = await _walletService.GetTransactionHistoryAsync(userId, 10);

        // Assert
        result.Should().HaveCount(10);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}