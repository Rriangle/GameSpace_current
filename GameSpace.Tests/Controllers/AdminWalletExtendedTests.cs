using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminWalletController 擴展功能測試
    /// 測試 EVoucherToken 與 EVoucherRedeemLog 管理功能
    /// </summary>
    public class AdminWalletExtendedTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletExtendedTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private AdminWalletController CreateController()
        {
            return new AdminWalletController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task EVoucherTokens_ReturnsViewWithTokens()
        {
            // Arrange
            var controller = CreateController();
            
            // 創建測試資料
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
                Token = "ABC123DEF456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsRevoked = false
            };
            await _context.EVoucherTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.EVoucherTokens();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EVoucherToken>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("ABC123DEF456", model[0].Token);
            Assert.False(model[0].IsRevoked);
        }

        [Fact]
        public async Task EVoucherTokenDetails_ValidId_ReturnsTokenDetails()
        {
            // Arrange
            var controller = CreateController();
            
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
                Token = "ABC123DEF456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsRevoked = false
            };
            await _context.EVoucherTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.EVoucherTokenDetails(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EVoucherToken>(viewResult.Model);
            Assert.Equal(1, model.TokenID);
            Assert.Equal("ABC123DEF456", model.Token);
        }

        [Fact]
        public async Task EVoucherRedeemLogs_ReturnsViewWithRedeemLogs()
        {
            // Arrange
            var controller = CreateController();
            
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

            var redeemLog = new EVoucherRedeemLog
            {
                RedeemID = 1,
                EVoucherID = evoucher.EVoucherID,
                UserID = user.UserID,
                ScannedAt = DateTime.UtcNow,
                Status = "Approved"
            };
            await _context.EVoucherRedeemLogs.AddAsync(redeemLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.EVoucherRedeemLogs();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EVoucherRedeemLog>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Approved", model[0].Status);
            Assert.Equal(1, model[0].UserID);
        }

        [Fact]
        public async Task EVoucherRedeemLogs_WithStatusFilter_ReturnsFilteredResults()
        {
            // Arrange
            var controller = CreateController();
            
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

            // 創建不同狀態的兌換記錄
            var redeemLogs = new List<EVoucherRedeemLog>
            {
                new EVoucherRedeemLog
                {
                    RedeemID = 1,
                    EVoucherID = evoucher.EVoucherID,
                    UserID = user.UserID,
                    ScannedAt = DateTime.UtcNow,
                    Status = "Approved"
                },
                new EVoucherRedeemLog
                {
                    RedeemID = 2,
                    EVoucherID = evoucher.EVoucherID,
                    UserID = user.UserID,
                    ScannedAt = DateTime.UtcNow.AddMinutes(-10),
                    Status = "Rejected"
                }
            };
            await _context.EVoucherRedeemLogs.AddRangeAsync(redeemLogs);
            await _context.SaveChangesAsync();

            // Act - 篩選 Approved 狀態
            var result = await controller.EVoucherRedeemLogs(status: "Approved");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EVoucherRedeemLog>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Approved", model[0].Status);
        }
    }
}