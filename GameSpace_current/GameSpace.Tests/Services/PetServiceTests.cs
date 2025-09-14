using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;

namespace GameSpace.Tests.Services;

/// <summary>
/// PetService 單元測試
/// </summary>
public class PetServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly PetService _petService;
    private readonly Fixture _fixture;

    public PetServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _petService = new PetService(_context);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetPetByUserIdAsync_當用戶有寵物時_應該返回寵物資訊()
    {
        // Arrange
        var userId = 1;
        var pet = new Pet
        {
            UserID = userId,
            Pet_Name = "測試史萊姆",
            Level = 1,
            Experience = 0,
            Hunger = 50,
            Mood = 50,
            Energy = 50,
            Cleanliness = 50,
            Health = 50,
            Skin_Color = "#ADD8E6",
            Background_Color = "粉藍"
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        // Act
        var result = await _petService.GetPetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserID.Should().Be(userId);
        result.Pet_Name.Should().Be("測試史萊姆");
    }

    [Fact]
    public async Task GetPetByUserIdAsync_當用戶沒有寵物時_應該返回null()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _petService.GetPetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePetAsync_應該成功創建寵物()
    {
        // Arrange
        var userId = 1;
        var petName = "新史萊姆";

        // Act
        var result = await _petService.CreatePetAsync(userId, petName);

        // Assert
        result.Should().NotBeNull();
        result.UserID.Should().Be(userId);
        result.Pet_Name.Should().Be(petName);
        result.Level.Should().Be(1);
        result.Experience.Should().Be(0);
        result.Hunger.Should().Be(50);
        result.Mood.Should().Be(50);
        result.Energy.Should().Be(50);
        result.Cleanliness.Should().Be(50);
        result.Health.Should().Be(50);
    }

    [Theory]
    [InlineData("餵食", 10, 5)]
    [InlineData("玩耍", 0, 10)]
    [InlineData("洗澡", 0, 0)]
    [InlineData("休息", 0, 0)]
    public async Task InteractWithPetAsync_應該正確更新寵物屬性(string action, int expectedHungerChange, int expectedMoodChange)
    {
        // Arrange
        var userId = 1;
        var pet = new Pet
        {
            UserID = userId,
            Pet_Name = "測試史萊姆",
            Level = 1,
            Experience = 0,
            Hunger = 50,
            Mood = 50,
            Energy = 50,
            Cleanliness = 50,
            Health = 50,
            Skin_Color = "#ADD8E6",
            Background_Color = "粉藍"
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var initialHunger = pet.Hunger;
        var initialMood = pet.Mood;

        // Act
        var result = await _petService.InteractWithPetAsync(userId, action);

        // Assert
        result.Should().BeTrue();
        
        var updatedPet = await _context.Pets.FirstAsync(p => p.UserID == userId);
        
        if (action == "餵食")
        {
            updatedPet.Hunger.Should().Be(Math.Min(100, initialHunger + expectedHungerChange));
            updatedPet.Mood.Should().Be(Math.Min(100, initialMood + expectedMoodChange));
        }
        else if (action == "玩耍")
        {
            updatedPet.Mood.Should().Be(Math.Min(100, initialMood + expectedMoodChange));
        }
    }

    [Fact]
    public async Task InteractWithPetAsync_當寵物不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _petService.InteractWithPetAsync(userId, "餵食");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePetSkinColorAsync_當點數足夠時_應該成功更新膚色()
    {
        // Arrange
        var userId = 1;
        var pet = new Pet
        {
            UserID = userId,
            Pet_Name = "測試史萊姆",
            Level = 1,
            Experience = 0,
            Hunger = 50,
            Mood = 50,
            Energy = 50,
            Cleanliness = 50,
            Health = 50,
            Skin_Color = "#ADD8E6",
            Background_Color = "粉藍"
        };

        var userWallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 3000
        };

        _context.Pets.Add(pet);
        _context.UserWallets.Add(userWallet);
        await _context.SaveChangesAsync();

        var newColor = "#FF0000";

        // Act
        var result = await _petService.UpdatePetSkinColorAsync(userId, newColor);

        // Assert
        result.Should().BeTrue();
        
        var updatedPet = await _context.Pets.FirstAsync(p => p.UserID == userId);
        updatedPet.Skin_Color.Should().Be(newColor);
        
        var updatedWallet = await _context.UserWallets.FirstAsync(w => w.User_Id == userId);
        updatedWallet.User_Point.Should().Be(1000); // 3000 - 2000
    }

    [Fact]
    public async Task UpdatePetSkinColorAsync_當點數不足時_應該返回false()
    {
        // Arrange
        var userId = 1;
        var pet = new Pet
        {
            UserID = userId,
            Pet_Name = "測試史萊姆",
            Level = 1,
            Experience = 0,
            Hunger = 50,
            Mood = 50,
            Energy = 50,
            Cleanliness = 50,
            Health = 50,
            Skin_Color = "#ADD8E6",
            Background_Color = "粉藍"
        };

        var userWallet = new UserWallet
        {
            User_Id = userId,
            User_Point = 1000 // 不足 2000 點
        };

        _context.Pets.Add(pet);
        _context.UserWallets.Add(userWallet);
        await _context.SaveChangesAsync();

        var newColor = "#FF0000";

        // Act
        var result = await _petService.UpdatePetSkinColorAsync(userId, newColor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePetBackgroundColorAsync_應該成功更新背景色()
    {
        // Arrange
        var userId = 1;
        var pet = new Pet
        {
            UserID = userId,
            Pet_Name = "測試史萊姆",
            Level = 1,
            Experience = 0,
            Hunger = 50,
            Mood = 50,
            Energy = 50,
            Cleanliness = 50,
            Health = 50,
            Skin_Color = "#ADD8E6",
            Background_Color = "粉藍"
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var newBackground = "紅色";

        // Act
        var result = await _petService.UpdatePetBackgroundColorAsync(userId, newBackground);

        // Assert
        result.Should().BeTrue();
        
        var updatedPet = await _context.Pets.FirstAsync(p => p.UserID == userId);
        updatedPet.Background_Color.Should().Be(newBackground);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}