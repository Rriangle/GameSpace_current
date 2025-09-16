using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using System.Text.Json;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// HealthController 整合測試
    /// 測試資料庫連線檢查與系統狀態端點
    /// </summary>
    public class HealthControllerIntegrationTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public HealthControllerIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private HealthController CreateController()
        {
            return new HealthController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task Database_HealthCheck_ReturnsOkStatus()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Database();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var healthResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.True(healthResponse.GetProperty("status").GetString() == "ok" || 
                       healthResponse.GetProperty("status").GetString() == "warning");
            Assert.True(healthResponse.GetProperty("database_connected").GetBoolean());
            Assert.Equal("MiniGame", healthResponse.GetProperty("area").GetString());
            
            // 檢查必要的資料表
            var tables = healthResponse.GetProperty("tables");
            Assert.True(tables.TryGetProperty("User_Wallet", out _));
            Assert.True(tables.TryGetProperty("CouponType", out _));
            Assert.True(tables.TryGetProperty("EVoucherType", out _));
        }

        [Fact]
        public async Task Status_SystemCheck_ReturnsSystemInfo()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Status();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var statusResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", statusResponse.GetProperty("status").GetString());
            Assert.Equal("MiniGame Admin 系統運行正常", statusResponse.GetProperty("message").GetString());
            Assert.Equal("MiniGame", statusResponse.GetProperty("area").GetString());
            
            // 檢查模組資訊
            var modules = statusResponse.GetProperty("modules");
            Assert.True(modules.TryGetProperty("User_Wallet", out _));
            Assert.True(modules.TryGetProperty("UserSignInStats", out _));
            Assert.True(modules.TryGetProperty("Pet", out _));
            Assert.True(modules.TryGetProperty("MiniGame", out _));
        }

        [Fact]
        public async Task Seed_DatabaseSeeding_ReturnsSuccessWithDetails()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Seed();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var seedResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("success", seedResponse.GetProperty("status").GetString());
            Assert.True(seedResponse.GetProperty("idempotent").GetBoolean());
            Assert.Equal(1000, seedResponse.GetProperty("batch_limit").GetInt32());
            Assert.Equal(200, seedResponse.GetProperty("target_per_table").GetInt32());
            Assert.Equal("zh-TW", seedResponse.GetProperty("language").GetString());
            
            // 檢查詳細結果
            var details = seedResponse.GetProperty("details");
            Assert.True(details.TryGetProperty("CouponType", out _));
            Assert.True(details.TryGetProperty("EVoucherType", out _));
        }

        [Fact] 
        public async Task Seed_IdempotentBehavior_DoesNotDuplicateData()
        {
            // Arrange
            var controller = CreateController();
            
            // 先執行一次播種
            await controller.Seed();
            var initialCouponTypeCount = await _context.CouponTypes.CountAsync();
            var initialEVoucherTypeCount = await _context.EVoucherTypes.CountAsync();

            // Act - 再次執行播種
            var result = await controller.Seed();

            // Assert
            var finalCouponTypeCount = await _context.CouponTypes.CountAsync();
            var finalEVoucherTypeCount = await _context.EVoucherTypes.CountAsync();
            
            // 如果已達到目標數量，不應該再增加記錄
            if (initialCouponTypeCount >= 200)
            {
                Assert.Equal(initialCouponTypeCount, finalCouponTypeCount);
            }
            
            if (initialEVoucherTypeCount >= 200)
            {
                Assert.Equal(initialEVoucherTypeCount, finalEVoucherTypeCount);
            }

            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var seedResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            Assert.Equal("success", seedResponse.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Tables_TableCounts_ReturnsCountsForAllMiniGameTables()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Tables();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var tablesResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", tablesResponse.GetProperty("status").GetString());
            Assert.Equal(11, tablesResponse.GetProperty("total_tables").GetInt32());
            Assert.Equal("MiniGame", tablesResponse.GetProperty("area").GetString());
            
            // 檢查資料表計數
            var tableCounts = tablesResponse.GetProperty("table_counts");
            Assert.True(tableCounts.TryGetProperty("User_Wallet", out _));
            Assert.True(tableCounts.TryGetProperty("CouponType", out _));
            Assert.True(tableCounts.TryGetProperty("Coupon", out _));
            Assert.True(tableCounts.TryGetProperty("EVoucherType", out _));
            Assert.True(tableCounts.TryGetProperty("EVoucher", out _));
            Assert.True(tableCounts.TryGetProperty("EVoucherToken", out _));
            Assert.True(tableCounts.TryGetProperty("EVoucherRedeemLog", out _));
            Assert.True(tableCounts.TryGetProperty("WalletHistory", out _));
            Assert.True(tableCounts.TryGetProperty("UserSignInStats", out _));
            Assert.True(tableCounts.TryGetProperty("Pet", out _));
            Assert.True(tableCounts.TryGetProperty("MiniGame", out _));
        }

        [Fact]
        public async Task FieldCoverage_DiagnosticsCheck_ReturnsFieldCoverageReport()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.FieldCoverage();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var coverageResponse = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("MiniGame", coverageResponse.GetProperty("area").GetString());
            
            // 檢查診斷資訊
            var adminPages = coverageResponse.GetProperty("admin_pages");
            Assert.True(adminPages.GetArrayLength() >= 8);
            
            var summary = coverageResponse.GetProperty("summary");
            Assert.Equal(8, summary.GetProperty("total_pages").GetInt32());
            Assert.True(summary.TryGetProperty("avg_coverage", out _));
            Assert.True(summary.TryGetProperty("recommendations", out _));
        }
    }
}