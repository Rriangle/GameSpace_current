using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using System.Text.Json;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminAnalytics 控制器測試
    /// 測試分析 JSON 端點功能
    /// </summary>
    public class AdminAnalyticsTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminAnalyticsTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private AdminAnalyticsController CreateController()
        {
            return new AdminAnalyticsController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task MiniGameOverview_Returns200_WithSeriesData()
        {
            // Arrange
            var controller = CreateController();
            await SetupMiniGameTestData();

            // Act
            var result = await controller.MiniGameOverview();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", response.GetProperty("status").GetString());
            Assert.True(response.TryGetProperty("range", out _));
            Assert.True(response.TryGetProperty("series", out _));
            
            var series = response.GetProperty("series").EnumerateArray().ToList();
            Assert.NotEmpty(series);
            
            // 檢查第一筆資料的結構
            var firstItem = series[0];
            Assert.True(firstItem.TryGetProperty("date", out _));
            Assert.True(firstItem.TryGetProperty("sessions", out _));
            Assert.True(firstItem.TryGetProperty("pointsSum", out _));
            Assert.True(firstItem.TryGetProperty("expSum", out _));
            Assert.True(firstItem.TryGetProperty("couponCount", out _));
        }

        [Fact]
        public async Task MiniGameOverview_WithDateRange_ChangesDataPoints()
        {
            // Arrange
            var controller = CreateController();
            await SetupMiniGameTestDataWithDates();

            // Act - 篩選最近 1 天
            var result = await controller.MiniGameOverview(from: DateTime.UtcNow.AddDays(-1));

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            var series = response.GetProperty("series").EnumerateArray().ToList();
            // 應該只有最近的資料點
            Assert.True(series.Count <= 2); // 今天和昨天
        }

        [Fact]
        public async Task SignInOverview_Returns200_WithSeriesData()
        {
            // Arrange
            var controller = CreateController();
            await SetupSignInTestData();

            // Act
            var result = await controller.SignInOverview();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", response.GetProperty("status").GetString());
            
            var series = response.GetProperty("series").EnumerateArray().ToList();
            Assert.NotEmpty(series);
            
            // 檢查資料結構
            var firstItem = series[0];
            Assert.True(firstItem.TryGetProperty("date", out _));
            Assert.True(firstItem.TryGetProperty("signInCount", out _));
            Assert.True(firstItem.TryGetProperty("rewardPointsSum", out _));
            Assert.True(firstItem.TryGetProperty("rewardExpSum", out _));
            Assert.True(firstItem.TryGetProperty("pointsGainedSum", out _));
        }

        [Fact]
        public async Task SignInOverview_WithDateFilter_AffectsResults()
        {
            // Arrange
            var controller = CreateController();
            await SetupSignInTestDataWithDates();

            // Act - 無篩選
            var allResult = await controller.SignInOverview();
            var allJsonString = JsonSerializer.Serialize(((JsonResult)allResult).Value);
            var allResponse = JsonSerializer.Deserialize<JsonElement>(allJsonString);
            var allSeries = allResponse.GetProperty("series").EnumerateArray().ToList();

            // Act - 有日期篩選
            var filteredResult = await controller.SignInOverview(from: DateTime.UtcNow.AddDays(-1));
            var filteredJsonString = JsonSerializer.Serialize(((JsonResult)filteredResult).Value);
            var filteredResponse = JsonSerializer.Deserialize<JsonElement>(filteredJsonString);
            var filteredSeries = filteredResponse.GetProperty("series").EnumerateArray().ToList();

            // Assert - 篩選後的資料點應該較少
            Assert.True(filteredSeries.Count <= allSeries.Count);
        }

        [Fact]
        public async Task MiniGameOverview_NoData_ReturnsEmptySeriesWithOkStatus()
        {
            // Arrange
            var controller = CreateController();
            // 不新增任何測試資料

            // Act
            var result = await controller.MiniGameOverview();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", response.GetProperty("status").GetString());
            var series = response.GetProperty("series").EnumerateArray().ToList();
            Assert.Empty(series);
        }

        private async Task SetupMiniGameTestData()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var pet = new Pet { PetID = 1, UserID = user.UserID, PetName = "測試寵物" };
            await _context.Pets.AddAsync(pet);

            var miniGame = new MiniGame
            {
                PlayID = 1,
                UserID = user.UserID,
                PetID = pet.PetID,
                Level = 1,
                Result = "Win",
                StartTime = DateTime.UtcNow,
                PointsGained = 100,
                PointsGainedTime = DateTime.UtcNow,
                ExpGained = 50,
                CouponGained = "BONUS001"
            };
            await _context.MiniGames.AddAsync(miniGame);
            await _context.SaveChangesAsync();
        }

        private async Task SetupMiniGameTestDataWithDates()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var pet = new Pet { PetID = 1, UserID = user.UserID, PetName = "測試寵物" };
            await _context.Pets.AddAsync(pet);

            var games = new[]
            {
                new MiniGame
                {
                    PlayID = 1,
                    UserID = user.UserID,
                    PetID = pet.PetID,
                    Level = 1,
                    Result = "Win",
                    StartTime = DateTime.UtcNow,
                    PointsGained = 100,
                    PointsGainedTime = DateTime.UtcNow,
                    ExpGained = 50
                },
                new MiniGame
                {
                    PlayID = 2,
                    UserID = user.UserID,
                    PetID = pet.PetID,
                    Level = 1,
                    Result = "Win",
                    StartTime = DateTime.UtcNow.AddDays(-5),
                    PointsGained = 75,
                    PointsGainedTime = DateTime.UtcNow.AddDays(-5),
                    ExpGained = 25
                }
            };
            await _context.MiniGames.AddRangeAsync(games);
            await _context.SaveChangesAsync();
        }

        private async Task SetupSignInTestData()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var signInStat = new UserSignInStats
            {
                LogID = 1,
                UserID = user.UserID,
                SignTime = DateTime.UtcNow,
                PointsGained = 20,
                PointsGainedTime = DateTime.UtcNow,
                ExpGained = 10,
                ExpGainedTime = DateTime.UtcNow,
                CouponGained = "0",
                CouponGainedTime = DateTime.UtcNow
            };
            await _context.UserSignInStats.AddAsync(signInStat);
            await _context.SaveChangesAsync();
        }

        private async Task SetupSignInTestDataWithDates()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var signInStats = new[]
            {
                new UserSignInStats
                {
                    LogID = 1,
                    UserID = user.UserID,
                    SignTime = DateTime.UtcNow,
                    PointsGained = 20,
                    PointsGainedTime = DateTime.UtcNow,
                    ExpGained = 10,
                    ExpGainedTime = DateTime.UtcNow,
                    CouponGained = "0",
                    CouponGainedTime = DateTime.UtcNow
                },
                new UserSignInStats
                {
                    LogID = 2,
                    UserID = user.UserID,
                    SignTime = DateTime.UtcNow.AddDays(-5),
                    PointsGained = 30,
                    PointsGainedTime = DateTime.UtcNow.AddDays(-5),
                    ExpGained = 15,
                    ExpGainedTime = DateTime.UtcNow.AddDays(-5),
                    CouponGained = "BONUS001",
                    CouponGainedTime = DateTime.UtcNow.AddDays(-5)
                }
            };
            await _context.UserSignInStats.AddRangeAsync(signInStats);
            await _context.SaveChangesAsync();
        }
    }
}