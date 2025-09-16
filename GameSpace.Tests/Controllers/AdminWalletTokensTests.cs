using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminWallet Tokens 頁面測試
    /// 驗證 EVoucherToken 列表功能
    /// </summary>
    public class AdminWalletTokensTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletTokensTests()
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
        public async Task Tokens_Returns200_WithoutParams()
        {
            // Arrange
            var controller = CreateController();
            
            // 創建基本測試資料
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var evoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "測試禮券類型",
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
            var result = await controller.Tokens();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<EVoucherToken>>(viewResult.Model);
            
            // 驗證資料存在
            Assert.Single(model);
            var tokenRecord = model.First();
            Assert.Equal("ABC123DEF456", tokenRecord.Token);
            Assert.Equal("TEST001", tokenRecord.EVoucher.EVoucherCode);
            Assert.False(tokenRecord.IsRevoked);
        }

        [Fact]
        public async Task Tokens_WithSearchFilter_ReducesResults()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var evoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "測試禮券類型",
                ValueAmount = 500,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(30),
                PointsCost = 800,
                TotalAvailable = 100
            };
            await _context.EVoucherTypes.AddAsync(evoucherType);

            var evouchers = new[]
            {
                new EVoucher
                {
                    EVoucherID = 1,
                    EVoucherCode = "SEARCH001",
                    EVoucherTypeID = evoucherType.EVoucherTypeID,
                    UserID = user.UserID,
                    AcquiredTime = DateTime.UtcNow
                },
                new EVoucher
                {
                    EVoucherID = 2,
                    EVoucherCode = "OTHER002",
                    EVoucherTypeID = evoucherType.EVoucherTypeID,
                    UserID = user.UserID,
                    AcquiredTime = DateTime.UtcNow
                }
            };
            await _context.EVouchers.AddRangeAsync(evouchers);

            var tokens = new[]
            {
                new EVoucherToken
                {
                    TokenID = 1,
                    EVoucherID = 1,
                    Token = "FINDME123",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsRevoked = false
                },
                new EVoucherToken
                {
                    TokenID = 2,
                    EVoucherID = 2,
                    Token = "HIDDEN456",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsRevoked = false
                }
            };
            await _context.EVoucherTokens.AddRangeAsync(tokens);
            await _context.SaveChangesAsync();

            // Act - 搜尋包含 "SEARCH" 的 EVoucherCode
            var result = await controller.Tokens(search: "SEARCH");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<EVoucherToken>>(viewResult.Model);
            
            // 應該只找到一筆記錄
            Assert.Single(model);
            Assert.Equal("SEARCH001", model.First().EVoucher.EVoucherCode);
        }

        [Fact]
        public async Task Tokens_WithRevokedFilter_ReturnsCorrectSubset()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var evoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "測試禮券類型",
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

            var tokens = new[]
            {
                new EVoucherToken
                {
                    TokenID = 1,
                    EVoucherID = evoucher.EVoucherID,
                    Token = "ACTIVE123",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsRevoked = false
                },
                new EVoucherToken
                {
                    TokenID = 2,
                    EVoucherID = evoucher.EVoucherID,
                    Token = "REVOKED456",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsRevoked = true
                }
            };
            await _context.EVoucherTokens.AddRangeAsync(tokens);
            await _context.SaveChangesAsync();

            // Act - 篩選已撤銷的 Token
            var result = await controller.Tokens(revoked: true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<EVoucherToken>>(viewResult.Model);
            
            // 應該只有一筆已撤銷的記錄
            Assert.Single(model);
            Assert.True(model.First().IsRevoked);
            Assert.Equal("REVOKED456", model.First().Token);
        }

        [Fact]
        public void Tokens_FieldCoverage_IncludesAllRequiredFields()
        {
            // Arrange - 根據 database.json EVoucherToken 表定義
            var requiredFields = new[] { "TokenID", "Token", "EVoucherID", "ExpiresAt", "IsRevoked" };
            
            // Act - 檢查 EVoucherToken 模型屬性
            var modelType = typeof(EVoucherToken);
            var properties = modelType.GetProperties().Select(p => p.Name).ToArray();
            
            // Assert - 驗證所有必要欄位存在
            foreach (var field in requiredFields)
            {
                Assert.Contains(field, properties);
            }
            
            // 特別驗證關鍵欄位的資料類型
            var tokenProperty = modelType.GetProperty("Token");
            Assert.NotNull(tokenProperty);
            Assert.Equal(typeof(string), tokenProperty.PropertyType);
            
            var expiresAtProperty = modelType.GetProperty("ExpiresAt");
            Assert.NotNull(expiresAtProperty);
            Assert.Equal(typeof(DateTime), expiresAtProperty.PropertyType);
            
            var isRevokedProperty = modelType.GetProperty("IsRevoked");
            Assert.NotNull(isRevokedProperty);
            Assert.Equal(typeof(bool), isRevokedProperty.PropertyType);
        }
    }
}