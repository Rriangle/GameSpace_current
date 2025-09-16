using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Areas.MiniGame.Filters;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Data;
using GameSpace.Models;
using System.Text.Json;
using Xunit;
using Moq;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// Batch 7 基礎設施測試
    /// 測試 ProblemDetails、快取和超時功能
    /// </summary>
    public class Batch7InfrastructureTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public Batch7InfrastructureTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        private AdminAnalyticsController CreateController()
        {
            return new AdminAnalyticsController(_context, _memoryCache);
        }

        public void Dispose()
        {
            _context.Dispose();
            _memoryCache.Dispose();
        }

        [Fact]
        public async Task MiniGameOverview_WithCacheOff_BypassesCache()
        {
            // Arrange
            var controller = CreateController();
            await SetupTestData();

            // Act - 第一次呼叫（cache=off）
            var result1 = await controller.MiniGameOverview(cache: "off");
            var jsonResult1 = Assert.IsType<JsonResult>(result1);
            var jsonString1 = JsonSerializer.Serialize(jsonResult1.Value);
            var response1 = JsonSerializer.Deserialize<JsonElement>(jsonString1);

            // Act - 第二次呼叫（cache=off）
            var result2 = await controller.MiniGameOverview(cache: "off");
            var jsonResult2 = Assert.IsType<JsonResult>(result2);
            var jsonString2 = JsonSerializer.Serialize(jsonResult2.Value);
            var response2 = JsonSerializer.Deserialize<JsonElement>(jsonString2);

            // Assert - 兩次呼叫都不使用快取
            Assert.Equal("ok", response1.GetProperty("status").GetString());
            Assert.Equal("ok", response2.GetProperty("status").GetString());
            Assert.False(response1.GetProperty("cached").GetBoolean());
            Assert.False(response2.GetProperty("cached").GetBoolean());
        }

        [Fact]
        public async Task SignInOverview_RepeatedCalls_UsesCache()
        {
            // Arrange
            var controller = CreateController();
            await SetupSignInTestData();

            // Act - 第一次呼叫（建立快取）
            var result1 = await controller.SignInOverview();
            var jsonResult1 = Assert.IsType<JsonResult>(result1);

            // Act - 第二次呼叫（應該使用快取）
            var result2 = await controller.SignInOverview();
            var jsonResult2 = Assert.IsType<JsonResult>(result2);

            // Assert - 兩次呼叫都成功
            Assert.IsType<JsonResult>(result1);
            Assert.IsType<JsonResult>(result2);
            
            // 驗證回應結構
            var jsonString = JsonSerializer.Serialize(jsonResult1.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            Assert.Equal("ok", response.GetProperty("status").GetString());
            Assert.True(response.TryGetProperty("series", out _));
        }

        [Fact]
        public void CacheInvalidate_ClearsAllMiniGameEntries()
        {
            // Arrange
            var controller = CreateController();
            var cache = new MiniGameCache(_memoryCache);
            
            // 建立一些快取項目
            cache.GetOrCreateAsync("MiniGame:test1", TimeSpan.FromMinutes(5), 
                _ => Task.FromResult("value1"), CancellationToken.None).Wait();
            cache.GetOrCreateAsync("MiniGame:test2", TimeSpan.FromMinutes(5), 
                _ => Task.FromResult("value2"), CancellationToken.None).Wait();

            // Act
            var result = controller.CacheInvalidate();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            var response = JsonSerializer.Deserialize<JsonElement>(jsonString);
            
            Assert.Equal("ok", response.GetProperty("status").GetString());
            Assert.Equal("快取已清除", response.GetProperty("message").GetString());
            Assert.True(response.TryGetProperty("before", out _));
            Assert.True(response.TryGetProperty("after", out _));
        }

        [Fact]
        public void MiniGameCache_MakeKey_CreatesConsistentKeys()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Path).Returns(new PathString("/MiniGame/AdminAnalytics/MiniGameOverview"));
            
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "from", "2023-01-01" },
                { "to", "2023-01-31" },
                { "cache", "off" }
            });
            mockRequest.Setup(r => r.Query).Returns(queryCollection);

            // Act
            var key = MiniGameCache.MakeKey(mockRequest.Object);

            // Assert
            Assert.StartsWith("MiniGame:/MiniGame/AdminAnalytics/MiniGameOverview:", key);
            Assert.Contains("from=2023-01-01", key);
            Assert.Contains("to=2023-01-31", key);
            Assert.Contains("cache=off", key);
        }

        [Fact]
        public void MiniGameCache_NormalizeQueryString_SortsAndTrimsParams()
        {
            // Arrange
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "z_param", "last" },
                { "a_param", "first" },
                { "empty_param", "" },
                { "  spaced  ", "  trimmed  " }
            });

            // Act
            var normalized = MiniGameCache.NormalizeQueryString(queryCollection);

            // Assert
            Assert.DoesNotContain("empty_param", normalized);
            Assert.Contains("a_param=first", normalized);
            Assert.Contains("spaced=trimmed", normalized);
            Assert.Contains("z_param=last", normalized);
            
            // 驗證排序（a_param 應該在 z_param 前面）
            var aIndex = normalized.IndexOf("a_param");
            var zIndex = normalized.IndexOf("z_param");
            Assert.True(aIndex < zIndex);
        }

        [Fact]
        public void ProblemDetailsFilter_TaskCanceledException_Returns408()
        {
            // Arrange
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.IsDevelopment()).Returns(false);
            mockEnvironment.Setup(e => e.IsProduction()).Returns(true);
            
            var filter = new MiniGameProblemDetailsFilter(mockEnvironment.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-123";
            httpContext.Request.Path = "/MiniGame/AdminAnalytics/Test";
            
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new TaskCanceledException("查詢超時")
            };

            // Act
            filter.OnException(exceptionContext);

            // Assert
            Assert.True(exceptionContext.ExceptionHandled);
            var objectResult = Assert.IsType<ObjectResult>(exceptionContext.Result);
            Assert.Equal(408, objectResult.StatusCode);
            
            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("系統錯誤", problemDetails.Title);
            Assert.Equal(408, problemDetails.Status);
            Assert.Equal("/MiniGame/AdminAnalytics/Test", problemDetails.Instance);
            Assert.Equal("test-trace-123", problemDetails.Extensions["traceId"]);
            Assert.Equal("MiniGame", problemDetails.Extensions["area"]);
        }

        private async Task SetupTestData()
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
    }
}