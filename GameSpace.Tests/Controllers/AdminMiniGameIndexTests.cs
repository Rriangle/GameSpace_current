using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminMiniGame Index 頁面測試
    /// 驗證 PointsGained 和 PointsGainedTime 顯示功能
    /// </summary>
    public class AdminMiniGameIndexTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminMiniGameIndexTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private AdminMiniGameController CreateController()
        {
            return new AdminMiniGameController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task Index_Returns200_WithMiniGameData()
        {
            // Arrange
            var controller = CreateController();
            
            // 創建測試資料
            var user = new User 
            { 
                UserID = 1, 
                UserName = "testuser", 
                UserAccount = "testuser", 
                UserPassword = "pass" 
            };
            await _context.Users.AddAsync(user);

            var pet = new Pet
            {
                PetID = 1,
                UserID = user.UserID,
                PetName = "測試寵物",
                Level = 5,
                Experience = 100
            };
            await _context.Pets.AddAsync(pet);

            var miniGame = new MiniGame
            {
                PlayID = 1,
                UserID = user.UserID,
                PetID = pet.PetID,
                Level = 2,
                MonsterCount = 5,
                SpeedMultiplier = 1.5m,
                Result = "Win",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddMinutes(-30),
                PointsGained = 150,
                PointsGainedTime = DateTime.UtcNow.AddMinutes(-30),
                ExpGained = 50,
                ExpGainedTime = DateTime.UtcNow.AddMinutes(-30),
                CouponGained = "BONUS001",
                Aborted = false
            };
            await _context.MiniGames.AddAsync(miniGame);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<MiniGame>>(viewResult.Model);
            
            // 驗證資料存在
            Assert.Single(model);
            var gameRecord = model.First();
            Assert.Equal(150, gameRecord.PointsGained);
            Assert.Equal("Win", gameRecord.Result);
            Assert.Equal("BONUS001", gameRecord.CouponGained);
        }

        [Fact]
        public async Task Index_WithPointsGainedData_ShowsRewardsWithTimestamp()
        {
            // Arrange
            var controller = CreateController();
            
            var user = new User { UserID = 1, UserName = "testuser", UserAccount = "testuser", UserPassword = "pass" };
            await _context.Users.AddAsync(user);

            var pet = new Pet { PetID = 1, UserID = user.UserID, PetName = "測試寵物" };
            await _context.Pets.AddAsync(pet);

            // 創建有點數獎勵的遊戲記錄
            var gameWithPoints = new MiniGame
            {
                PlayID = 1,
                UserID = user.UserID,
                PetID = pet.PetID,
                Level = 1,
                Result = "Win",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                PointsGained = 100,
                PointsGainedTime = DateTime.UtcNow.AddHours(-1),
                ExpGained = 25
            };

            // 創建無點數獎勵的遊戲記錄
            var gameWithoutPoints = new MiniGame
            {
                PlayID = 2,
                UserID = user.UserID,
                PetID = pet.PetID,
                Level = 1,
                Result = "Lose",
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = DateTime.UtcNow.AddMinutes(-15),
                PointsGained = 0,
                PointsGainedTime = DateTime.UtcNow.AddMinutes(-15),
                ExpGained = 0
            };

            await _context.MiniGames.AddRangeAsync(new[] { gameWithPoints, gameWithoutPoints });
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<MiniGame>>(viewResult.Model);
            
            // 驗證兩筆記錄都存在
            Assert.Equal(2, model.Count());
            
            // 驗證有點數的記錄
            var winGame = model.First(g => g.Result == "Win");
            Assert.Equal(100, winGame.PointsGained);
            Assert.True(winGame.PointsGainedTime != default(DateTime));
            
            // 驗證無點數的記錄
            var loseGame = model.First(g => g.Result == "Lose");
            Assert.Equal(0, loseGame.PointsGained);
            Assert.True(loseGame.PointsGainedTime != default(DateTime)); // 時間戳仍應存在
        }

        [Fact]
        public async Task Index_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var controller = CreateController();
            
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
                    StartTime = DateTime.UtcNow.AddDays(-1),
                    PointsGained = 50,
                    PointsGainedTime = DateTime.UtcNow.AddDays(-1)
                },
                new MiniGame
                {
                    PlayID = 2,
                    UserID = user.UserID,
                    PetID = pet.PetID,
                    Level = 2,
                    Result = "Lose",
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    PointsGained = 0,
                    PointsGainedTime = DateTime.UtcNow.AddHours(-1)
                }
            };
            await _context.MiniGames.AddRangeAsync(games);
            await _context.SaveChangesAsync();

            // Act - 篩選勝利記錄
            var result = await controller.Index(result: "Win");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<MiniGame>>(viewResult.Model);
            
            // 應該只有一筆勝利記錄
            Assert.Single(model);
            Assert.Equal("Win", model.First().Result);
            Assert.Equal(50, model.First().PointsGained);
        }
    }
}