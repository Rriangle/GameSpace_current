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
    /// Batch 5 匯出功能測試
    /// 測試 EVoucherToken 和 EVoucherRedeemLog 的 CSV/JSON 匯出
    /// </summary>
    public class Batch5ExportTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public Batch5ExportTests()
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
        public async Task TokensExportCsv_Returns200_WithCsvContent()
        {
            // Arrange
            var controller = CreateController();
            await SetupTokenTestData();

            // Act
            var result = await controller.TokensExportCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.StartsWith("evoucher_tokens_", fileResult.FileDownloadName);
            Assert.EndsWith(".csv", fileResult.FileDownloadName);
            
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("TokenID,Token,EVoucherID", csvContent); // CSV 標題
            Assert.Contains("ABC123", csvContent); // Token 內容
        }

        [Fact]
        public async Task TokensExportJson_Returns200_WithJsonContent()
        {
            // Arrange
            var controller = CreateController();
            await SetupTokenTestData();

            // Act
            var result = await controller.TokensExportJson();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(jsonString);
            
            Assert.Single(jsonArray);
            var record = jsonArray[0];
            Assert.Equal(1, record.GetProperty("TokenID").GetInt32());
            Assert.Equal("ABC123", record.GetProperty("Token").GetString());
            Assert.Equal("TEST001", record.GetProperty("EVoucherCode").GetString());
            Assert.False(record.GetProperty("IsRevoked").GetBoolean());
        }

        [Fact]
        public async Task TokensExportCsv_WithFilters_ProducesConsistentSubsets()
        {
            // Arrange
            var controller = CreateController();
            await SetupMultipleTokenTestData();

            // Act - 搜尋特定 Token
            var result = await controller.TokensExportCsv(search: "SEARCH");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            
            // 應該只包含搜尋結果
            Assert.Contains("SEARCH001", csvContent);
            Assert.DoesNotContain("OTHER002", csvContent);
        }

        [Fact]
        public async Task RedeemLogsExportCsv_Returns200_WithCsvContent()
        {
            // Arrange
            var controller = CreateController();
            await SetupRedeemLogTestData();

            // Act
            var result = await controller.RedeemLogsExportCsv();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.StartsWith("evoucher_redeemlogs_", fileResult.FileDownloadName);
            Assert.EndsWith(".csv", fileResult.FileDownloadName);
            
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("RedeemID,TokenID,Token", csvContent); // CSV 標題
            Assert.Contains("Approved", csvContent); // 狀態內容
        }

        [Fact]
        public async Task RedeemLogsExportJson_WithStatusFilter_ReturnsFilteredResults()
        {
            // Arrange
            var controller = CreateController();
            await SetupMultipleRedeemLogTestData();

            // Act - 篩選 Approved 狀態
            var result = await controller.RedeemLogsExportJson(status: "Approved");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(jsonString);
            
            // 應該只有 Approved 記錄
            Assert.Single(jsonArray);
            Assert.Equal("Approved", jsonArray[0].GetProperty("Status").GetString());
        }

        [Fact]
        public async Task RedeemLogsExportCsv_WithDateFilter_ReturnsCorrectTimeRange()
        {
            // Arrange
            var controller = CreateController();
            await SetupTimeBasedRedeemLogTestData();

            // Act - 篩選最近 1 天
            var result = await controller.RedeemLogsExportCsv(from: DateTime.UtcNow.AddDays(-1));

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            var csvContent = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
            
            // 驗證 CSV 結構和內容
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length); // 1 標題 + 1 資料行（只有新記錄）
            Assert.Contains("新記錄", csvContent);
            Assert.DoesNotContain("舊記錄", csvContent);
        }

        private async Task SetupTokenTestData()
        {
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
            await _context.SaveChangesAsync();
        }

        private async Task SetupMultipleTokenTestData()
        {
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
                    Token = "SEARCH123",
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
        }

        private async Task SetupRedeemLogTestData()
        {
            await SetupTokenTestData();
            
            var redeemLog = new EVoucherRedeemLog
            {
                RedeemID = 1,
                EVoucherID = 1,
                TokenID = 1,
                UserID = 1,
                ScannedAt = DateTime.UtcNow,
                Status = "Approved"
            };
            await _context.EVoucherRedeemLogs.AddAsync(redeemLog);
            await _context.SaveChangesAsync();
        }

        private async Task SetupMultipleRedeemLogTestData()
        {
            await SetupTokenTestData();
            
            var redeemLogs = new[]
            {
                new EVoucherRedeemLog
                {
                    RedeemID = 1,
                    EVoucherID = 1,
                    TokenID = 1,
                    UserID = 1,
                    ScannedAt = DateTime.UtcNow,
                    Status = "Approved"
                },
                new EVoucherRedeemLog
                {
                    RedeemID = 2,
                    EVoucherID = 1,
                    TokenID = 1,
                    UserID = 1,
                    ScannedAt = DateTime.UtcNow.AddMinutes(-10),
                    Status = "Rejected"
                }
            };
            await _context.EVoucherRedeemLogs.AddRangeAsync(redeemLogs);
            await _context.SaveChangesAsync();
        }

        private async Task SetupTimeBasedRedeemLogTestData()
        {
            await SetupTokenTestData();
            
            var redeemLogs = new[]
            {
                new EVoucherRedeemLog
                {
                    RedeemID = 1,
                    EVoucherID = 1,
                    TokenID = 1,
                    UserID = 1,
                    ScannedAt = DateTime.UtcNow,
                    Status = "新記錄"
                },
                new EVoucherRedeemLog
                {
                    RedeemID = 2,
                    EVoucherID = 1,
                    TokenID = 1,
                    UserID = 1,
                    ScannedAt = DateTime.UtcNow.AddDays(-5),
                    Status = "舊記錄"
                }
            };
            await _context.EVoucherRedeemLogs.AddRangeAsync(redeemLogs);
            await _context.SaveChangesAsync();
        }
    }
}