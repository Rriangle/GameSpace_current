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
    /// 錢包歷史匯出功能測試
    /// 測試 CSV 和 JSON 匯出端點
    /// </summary>
    public class WalletHistoryExportTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public WalletHistoryExportTests()
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
        public async Task HistoryExportCsv_Returns200_WithCsvContent()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var history = new WalletHistory
            {
                LogID = 1,
                UserID = user.UserID,
                ChangeType = "Point",
                PointsChanged = 100,
                ItemCode = "SIGNIN001",
                Description = "每日簽到獲得",
                ChangeTime = DateTime.UtcNow
            };
            await _context.WalletHistories.AddAsync(history);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.HistoryExportCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.StartsWith("wallet_history_", fileResult.FileDownloadName);
            Assert.EndsWith(".csv", fileResult.FileDownloadName);
            
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("LogID,UserID,UserName", csvContent); // CSV 標題
            Assert.Contains("每日簽到獲得", csvContent); // 資料內容
        }

        [Fact]
        public async Task HistoryExportJson_Returns200_WithJsonContent()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var history = new WalletHistory
            {
                LogID = 1,
                UserID = user.UserID,
                ChangeType = "Coupon",
                PointsChanged = -500,
                ItemCode = "DISC001",
                Description = "兌換優惠券",
                ChangeTime = DateTime.UtcNow
            };
            await _context.WalletHistories.AddAsync(history);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.HistoryExportJson();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(jsonString);
            
            Assert.Single(jsonArray);
            var record = jsonArray[0];
            Assert.Equal(1, record.GetProperty("LogID").GetInt32());
            Assert.Equal("Coupon", record.GetProperty("ChangeType").GetString());
            Assert.Equal(-500, record.GetProperty("PointsChanged").GetInt32());
            Assert.Equal("DISC001", record.GetProperty("ItemCode").GetString());
            Assert.Equal("兌換優惠券", record.GetProperty("Description").GetString());
        }

        [Fact]
        public async Task HistoryExportCsv_WithFilters_ProducesConsistentSubsets()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var histories = new[]
            {
                new WalletHistory
                {
                    LogID = 1,
                    UserID = user.UserID,
                    ChangeType = "Point",
                    PointsChanged = 20,
                    Description = "簽到獎勵",
                    ChangeTime = DateTime.UtcNow.AddDays(-1)
                },
                new WalletHistory
                {
                    LogID = 2,
                    UserID = user.UserID,
                    ChangeType = "Coupon",
                    PointsChanged = -100,
                    Description = "兌換優惠券",
                    ChangeTime = DateTime.UtcNow
                }
            };
            await _context.WalletHistories.AddRangeAsync(histories);
            await _context.SaveChangesAsync();

            // Act - 篩選 Point 類型
            var result = await controller.HistoryExportCsv(type: "Point");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            
            // 應該只包含 Point 類型的記錄
            Assert.Contains("簽到獎勵", csvContent);
            Assert.DoesNotContain("兌換優惠券", csvContent);
        }

        [Fact]
        public async Task HistoryExportJson_WithDateFilter_ReturnsFilteredResults()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var histories = new[]
            {
                new WalletHistory
                {
                    LogID = 1,
                    UserID = user.UserID,
                    ChangeType = "Point",
                    PointsChanged = 20,
                    Description = "舊記錄",
                    ChangeTime = DateTime.UtcNow.AddDays(-5)
                },
                new WalletHistory
                {
                    LogID = 2,
                    UserID = user.UserID,
                    ChangeType = "Point",
                    PointsChanged = 30,
                    Description = "新記錄",
                    ChangeTime = DateTime.UtcNow
                }
            };
            await _context.WalletHistories.AddRangeAsync(histories);
            await _context.SaveChangesAsync();

            // Act - 篩選最近 2 天
            var result = await controller.HistoryExportJson(from: DateTime.UtcNow.AddDays(-2));

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(jsonString);
            
            // 應該只有一筆新記錄
            Assert.Single(jsonArray);
            Assert.Equal("新記錄", jsonArray[0].GetProperty("Description").GetString());
        }

        [Fact]
        public async Task HistoryExportCsv_LargeDataSet_StreamsCorrectly()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            // 創建較多記錄來測試效能
            var histories = new List<WalletHistory>();
            for (int i = 1; i <= 50; i++)
            {
                histories.Add(new WalletHistory
                {
                    LogID = i,
                    UserID = user.UserID,
                    ChangeType = i % 2 == 0 ? "Point" : "Coupon",
                    PointsChanged = i * 10,
                    Description = $"測試記錄 {i}",
                    ChangeTime = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await _context.WalletHistories.AddRangeAsync(histories);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.HistoryExportCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            
            // 驗證 CSV 結構
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(51, lines.Length); // 1 標題行 + 50 資料行
            Assert.StartsWith("LogID,UserID,UserName", lines[0]); // 標題行
            Assert.Contains("測試記錄", csvContent); // 資料內容
        }
    }
}