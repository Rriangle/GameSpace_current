using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// Batch 3 整合測試
    /// 測試 UserSignInStats, EVoucherRedeemLog, Pet 頁面功能
    /// </summary>
    public class Batch3IntegrationTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public Batch3IntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task UserSignInStats_Index_Returns200_ShowsPointsGainedAndTime()
        {
            // Arrange
            var controller = new AdminSignInStatsController(_context);
            
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

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserSignInStats>>(viewResult.Model);
            
            Assert.Single(model);
            var stat = model.First();
            Assert.Equal(20, stat.PointsGained);
            Assert.True(stat.PointsGainedTime != default(DateTime));
        }

        [Fact]
        public async Task UserSignInStats_Statistics_Returns200_IncludesPointsInAggregates()
        {
            // Arrange
            var controller = new AdminSignInStatsController(_context);
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var signInStats = new[]
            {
                new UserSignInStats
                {
                    LogID = 1,
                    UserID = user.UserID,
                    SignTime = DateTime.UtcNow.AddDays(-1),
                    PointsGained = 20,
                    PointsGainedTime = DateTime.UtcNow.AddDays(-1),
                    ExpGained = 10,
                    ExpGainedTime = DateTime.UtcNow.AddDays(-1),
                    CouponGained = "0",
                    CouponGainedTime = DateTime.UtcNow.AddDays(-1)
                },
                new UserSignInStats
                {
                    LogID = 2,
                    UserID = user.UserID,
                    SignTime = DateTime.UtcNow,
                    PointsGained = 30,
                    PointsGainedTime = DateTime.UtcNow,
                    ExpGained = 15,
                    ExpGainedTime = DateTime.UtcNow,
                    CouponGained = "BONUS001",
                    CouponGainedTime = DateTime.UtcNow
                }
            };
            await _context.UserSignInStats.AddRangeAsync(signInStats);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.Statistics();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            
            // 驗證 ViewBag 包含聚合資料
            Assert.NotNull(viewResult.ViewData["TotalSignIns"]);
            Assert.NotNull(viewResult.ViewData["TotalPoints"]);
            Assert.Equal(50, viewResult.ViewData["TotalPoints"]); // 20 + 30 = 50
        }

        [Fact]
        public async Task RedeemLogs_Returns200_WithSeededData()
        {
            // Arrange
            var controller = new AdminWalletController(_context);
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

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
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.RedeemLogs();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<EVoucherRedeemLog>>(viewResult.Model);
            
            Assert.Single(model);
            var log = model.First();
            Assert.Equal("Approved", log.Status);
            Assert.Equal(1, log.TokenID);
        }

        [Fact]
        public async Task RedeemLogs_WithFilters_ChangesResultCounts()
        {
            // Arrange
            var controller = new AdminWalletController(_context);
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

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
                EVoucherCode = "SEARCH001",
                EVoucherTypeID = evoucherType.EVoucherTypeID,
                UserID = user.UserID,
                AcquiredTime = DateTime.UtcNow
            };
            await _context.EVouchers.AddAsync(evoucher);

            var token = new EVoucherToken
            {
                TokenID = 1,
                EVoucherID = evoucher.EVoucherID,
                Token = "FINDME123",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsRevoked = false
            };
            await _context.EVoucherTokens.AddAsync(token);

            var redeemLogs = new[]
            {
                new EVoucherRedeemLog
                {
                    RedeemID = 1,
                    EVoucherID = evoucher.EVoucherID,
                    TokenID = token.TokenID,
                    UserID = user.UserID,
                    ScannedAt = DateTime.UtcNow,
                    Status = "Approved"
                },
                new EVoucherRedeemLog
                {
                    RedeemID = 2,
                    EVoucherID = evoucher.EVoucherID,
                    TokenID = token.TokenID,
                    UserID = user.UserID,
                    ScannedAt = DateTime.UtcNow.AddMinutes(-10),
                    Status = "Rejected"
                }
            };
            await _context.EVoucherRedeemLogs.AddRangeAsync(redeemLogs);
            await _context.SaveChangesAsync();

            // Act - 篩選 Approved 狀態
            var result = await controller.RedeemLogs(status: "Approved");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<EVoucherRedeemLog>>(viewResult.Model);
            
            Assert.Single(model);
            Assert.Equal("Approved", model.First().Status);
        }

        [Fact]
        public async Task RedeemLogDetail_ValidId_Returns200_ShowsAllFields()
        {
            // Arrange
            var controller = new AdminWalletController(_context);
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

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
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.RedeemLogDetail(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EVoucherRedeemLog>(viewResult.Model);
            
            // 驗證所有關鍵欄位
            Assert.Equal(1, model.RedeemID);
            Assert.Equal(1, model.EVoucherID);
            Assert.Equal(1, model.TokenID);
            Assert.Equal(1, model.UserID);
            Assert.Equal("Approved", model.Status);
            Assert.True(model.ScannedAt != default(DateTime));
        }

        [Fact]
        public async Task RedeemLogDetail_NonExistingId_Returns404()
        {
            // Arrange
            var controller = new AdminWalletController(_context);

            // Act
            var result = await controller.RedeemLogDetail(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("找不到指定的兌換記錄", notFoundResult.Value);
        }

        [Fact]
        public async Task Pet_Details_Returns200_ShowsColorSwatches()
        {
            // Arrange
            var controller = new AdminPetController(_context);
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var pet = new Pet
            {
                PetID = 1,
                UserID = user.UserID,
                PetName = "彩色寵物",
                Level = 5,
                Experience = 100,
                Health = 80,
                SkinColor = "#FF5733",
                BackgroundColor = "#33FF57",
                PointsChanged_SkinColor = 2000,
                PointsChangedTime_SkinColor = DateTime.UtcNow.AddDays(-1),
                PointsChanged_BackgroundColor = 1000,
                BackgroundColorChangedTime = DateTime.UtcNow.AddDays(-2)
            };
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Pet>(viewResult.Model);
            
            // 驗證顏色和消費記錄欄位
            Assert.Equal("#FF5733", model.SkinColor);
            Assert.Equal("#33FF57", model.BackgroundColor);
            Assert.Equal(2000, model.PointsChanged_SkinColor);
            Assert.Equal(1000, model.PointsChanged_BackgroundColor);
            Assert.True(model.PointsChangedTime_SkinColor != default(DateTime));
            Assert.True(model.BackgroundColorChangedTime != default(DateTime));
        }
    }
}