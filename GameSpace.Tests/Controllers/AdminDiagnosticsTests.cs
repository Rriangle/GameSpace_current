using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GameSpace.Areas.MiniGame.Controllers;
using System.Text.Json;
using Xunit;
using Moq;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminDiagnostics 控制器測試
    /// 測試欄位覆蓋率診斷功能
    /// </summary>
    public class AdminDiagnosticsTests
    {
        private AdminDiagnosticsController CreateController()
        {
            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.ContentRootPath).Returns("/workspace/GameSpace_current/GameSpace");
            return new AdminDiagnosticsController(mockEnvironment.Object);
        }

        [Fact]
        public async Task FieldCoverage_Returns200_WithAllMiniGameTables()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.FieldCoverage();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("MiniGame", response.GetProperty("area").GetString());
            Assert.Equal(11, response.GetProperty("total_tables").GetInt32());
            
            // 檢查是否包含所有目標資料表
            var tables = response.GetProperty("tables").EnumerateArray();
            var tableNames = tables.Select(t => t.GetProperty("table").GetString()).ToList();
            
            var expectedTables = new[]
            {
                "User_Wallet", "CouponType", "Coupon", "EVoucherType", "EVoucher",
                "EVoucherToken", "EVoucherRedeemLog", "WalletHistory", "UserSignInStats",
                "Pet", "MiniGame"
            };
            
            foreach (var expectedTable in expectedTables)
            {
                Assert.Contains(expectedTable, tableNames);
            }
            
            // 檢查摘要資訊
            var summary = response.GetProperty("summary");
            Assert.True(summary.TryGetProperty("avg_entity_coverage", out _));
            Assert.True(summary.TryGetProperty("avg_view_coverage", out _));
            Assert.True(summary.TryGetProperty("critical_gaps", out _));
        }

        [Fact]
        public async Task FieldCoverage_TableAnalysis_IncludesRequiredFields()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.FieldCoverage();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            var tables = response.GetProperty("tables").EnumerateArray();
            
            // 檢查每個資料表都有必要的分析欄位
            foreach (var table in tables)
            {
                Assert.True(table.TryGetProperty("table", out _));
                Assert.True(table.TryGetProperty("schemaCount", out _));
                Assert.True(table.TryGetProperty("entityCount", out _));
                Assert.True(table.TryGetProperty("viewFieldCount", out _));
                Assert.True(table.TryGetProperty("missingInEntity", out _));
                Assert.True(table.TryGetProperty("missingInView", out _));
                Assert.True(table.TryGetProperty("notes", out _));
                
                // 驗證計數為非負數
                Assert.True(table.GetProperty("schemaCount").GetInt32() >= 0);
                Assert.True(table.GetProperty("entityCount").GetInt32() >= 0);
                Assert.True(table.GetProperty("viewFieldCount").GetInt32() >= 0);
            }
        }

        [Fact]
        public async Task FieldCoverage_ErrorHandling_ReturnsErrorJson()
        {
            // Arrange - 使用無效路徑模擬錯誤
            var mockEnvironment = new Mock<IHostEnvironment>();
            mockEnvironment.Setup(e => e.ContentRootPath).Returns("/invalid/path");
            var controller = new AdminDiagnosticsController(mockEnvironment.Object);

            // Act
            var result = await controller.FieldCoverage();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.True(response.TryGetProperty("error", out _));
            Assert.True(response.TryGetProperty("message", out _));
            Assert.True(response.TryGetProperty("timestamp", out _));
        }
    }
}