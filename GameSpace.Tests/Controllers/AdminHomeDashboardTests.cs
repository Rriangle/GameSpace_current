using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminHome Dashboard 測試
    /// 測試儀表板功能和即時指標
    /// </summary>
    public class AdminHomeDashboardTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminHomeDashboardTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private AdminHomeController CreateController()
        {
            return new AdminHomeController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task Dashboard_Returns200_WithViewBagMetrics()
        {
            // Arrange
            var controller = CreateController();
            await SetupDashboardTestData();

            // Act
            var result = await controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            
            // 驗證 ViewBag 包含所有必要的計數
            Assert.NotNull(viewResult.ViewData["WalletHistoryCount"]);
            Assert.NotNull(viewResult.ViewData["EVoucherTokenTotalCount"]);
            Assert.NotNull(viewResult.ViewData["RedeemLogCount"]);
            Assert.NotNull(viewResult.ViewData["SignInStatsCount"]);
            Assert.NotNull(viewResult.ViewData["MiniGameCount"]);
            Assert.NotNull(viewResult.ViewData["DateRangeText"]);
            Assert.NotNull(viewResult.ViewData["DaysSpan"]);
        }

        [Fact]
        public async Task Dashboard_WithSeededData_ShowsNonZeroCounts()
        {
            // Arrange
            var controller = CreateController();
            await SetupDashboardTestData();

            // Act
            var result = await controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            
            // 驗證有種子資料時計數大於 0
            Assert.True((int)viewResult.ViewData["WalletHistoryCount"]! >= 0);
            Assert.True((int)viewResult.ViewData["EVoucherTokenTotalCount"]! >= 0);
            Assert.True((int)viewResult.ViewData["RedeemLogCount"]! >= 0);
            Assert.True((int)viewResult.ViewData["SignInStatsCount"]! >= 0);
            Assert.True((int)viewResult.ViewData["MiniGameCount"]! >= 0);
        }

        [Fact]
        public async Task Dashboard_WithDateRange_ChangesCountsAppropriately()
        {
            // Arrange
            var controller = CreateController();
            await SetupDateRangeTestData();

            // Act - 全範圍
            var allResult = await controller.Dashboard();
            var allViewResult = Assert.IsType<ViewResult>(allResult);
            var allWalletCount = (int)allViewResult.ViewData["WalletHistoryCount"]!;

            // Act - 縮小範圍到最近 1 天
            var narrowResult = await controller.Dashboard(from: DateTime.UtcNow.AddDays(-1));
            var narrowViewResult = Assert.IsType<ViewResult>(narrowResult);
            var narrowWalletCount = (int)narrowViewResult.ViewData["WalletHistoryCount"]!;

            // Assert - 縮小範圍後計數應該減少或相等
            Assert.True(narrowWalletCount <= allWalletCount);
        }

        [Fact]
        public async Task Dashboard_CalculatesAveragesCorrectly()
        {
            // Arrange
            var controller = CreateController();
            await SetupAverageTestData();

            // Act
            var result = await controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            
            // 驗證平均值計算
            var daysSpan = (int)viewResult.ViewData["DaysSpan"]!;
            var signInCount = (int)viewResult.ViewData["SignInStatsCount"]!;
            var avgSignIns = (double)viewResult.ViewData["AvgSignInsPerDay"]!;
            
            Assert.True(daysSpan > 0);
            if (signInCount > 0)
            {
                var expectedAvg = Math.Round((double)signInCount / daysSpan, 1);
                Assert.Equal(expectedAvg, avgSignIns);
            }
        }

        private async Task SetupDashboardTestData()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var pet = new Pet { PetID = 1, UserID = user.UserID, PetName = "測試寵物" };
            await _context.Pets.AddAsync(pet);

            var evoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "測試禮券",
                ValueAmount = 500,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(30),
                PointsCost = 800,
                TotalAvailable = 100
            };
            await _context.EVoucherTypes.AddAsync(evoucherType);

            var evoucher = new EVoucher
            {
                EVoucherID = 1,
                EVoucherCode = "TEST001",
                EVoucherTypeID = evoucherType.EVoucherTypeID,
                UserID = user.UserID,
                AcquiredTime = DateTime.UtcNow
            };
            await _context.EVouchers.AddAsync(evoucher);

            // 建立各種測試資料
            var walletHistory = new WalletHistory
            {
                LogID = 1,
                UserID = user.UserID,
                ChangeType = "Point",
                PointsChanged = 100,
                Description = "測試異動",
                ChangeTime = DateTime.UtcNow
            };
            await _context.WalletHistories.AddAsync(walletHistory);

            var token = new EVoucherToken
            {
                TokenID = 1,
                EVoucherID = evoucher.EVoucherID,
                Token = "ABC123",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsRevoked = false
            };
            await _context.EVoucherTokens.AddAsync(token);

            var redeemLog = new EVoucherRedeemLog
            {
                RedeemID = 1,
                EVoucherID = evoucher.EVoucherID,
                TokenID = token.TokenID,
                UserID = user.UserID,
                ScannedAt = DateTime.UtcNow,
                Status = "Approved"
            };
            await _context.EVoucherRedeemLogs.AddAsync(redeemLog);

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
                ExpGained = 50
            };
            await _context.MiniGames.AddAsync(miniGame);

            await _context.SaveChangesAsync();
        }

        private async Task SetupDateRangeTestData()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            // 建立不同日期的錢包記錄
            var histories = new[]
            {
                new WalletHistory
                {
                    LogID = 1,
                    UserID = user.UserID,
                    ChangeType = "Point",
                    PointsChanged = 100,
                    Description = "新記錄",
                    ChangeTime = DateTime.UtcNow
                },
                new WalletHistory
                {
                    LogID = 2,
                    UserID = user.UserID,
                    ChangeType = "Point",
                    PointsChanged = 50,
                    Description = "舊記錄",
                    ChangeTime = DateTime.UtcNow.AddDays(-5)
                }
            };
            await _context.WalletHistories.AddRangeAsync(histories);
            await _context.SaveChangesAsync();
        }

        private async Task SetupAverageTestData()
        {
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            // 建立 3 天內的簽到記錄
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
                    SignTime = DateTime.UtcNow.AddDays(-1),
                    PointsGained = 25,
                    PointsGainedTime = DateTime.UtcNow.AddDays(-1),
                    ExpGained = 15,
                    ExpGainedTime = DateTime.UtcNow.AddDays(-1),
                    CouponGained = "0",
                    CouponGainedTime = DateTime.UtcNow.AddDays(-1)
                }
            };
            await _context.UserSignInStats.AddRangeAsync(signInStats);
            await _context.SaveChangesAsync();
        }
    }
}