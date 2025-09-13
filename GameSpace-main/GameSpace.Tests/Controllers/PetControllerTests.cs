using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json;

namespace GameSpace.Tests.Controllers;

/// <summary>
/// PetController 整合測試
/// </summary>
public class PetControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly GameSpaceDbContext _context;

    public PetControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 移除現有的 DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameSpaceDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 添加 In-Memory 資料庫
                services.AddDbContext<GameSpaceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
        _context = _factory.Services.GetRequiredService<GameSpaceDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetPet_當寵物存在時_應該返回寵物資訊()
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
        var response = await _client.GetAsync($"/api/pet/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var petData = JsonSerializer.Deserialize<Pet>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        petData.Should().NotBeNull();
        petData.UserID.Should().Be(userId);
        petData.Pet_Name.Should().Be("測試史萊姆");
    }

    [Fact]
    public async Task GetPet_當寵物不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;

        // Act
        var response = await _client.GetAsync($"/api/pet/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePet_應該成功創建寵物()
    {
        // Arrange
        var userId = 1;
        var petName = "新史萊姆";

        // Act
        var response = await _client.PostAsJsonAsync($"/api/pet/{userId}", new { petName });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var petData = JsonSerializer.Deserialize<Pet>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        petData.Should().NotBeNull();
        petData.UserID.Should().Be(userId);
        petData.Pet_Name.Should().Be(petName);
    }

    [Fact]
    public async Task InteractWithPet_當寵物存在時_應該成功互動()
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

        var action = "餵食";

        // Act
        var response = await _client.PostAsJsonAsync($"/api/pet/{userId}/interact", new { action });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task InteractWithPet_當寵物不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var action = "餵食";

        // Act
        var response = await _client.PostAsJsonAsync($"/api/pet/{userId}/interact", new { action });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSkinColor_當點數足夠時_應該成功更新膚色()
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
        var response = await _client.PutAsJsonAsync($"/api/pet/{userId}/skin-color", new { color = newColor });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task UpdateSkinColor_當點數不足時_應該返回400()
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
        var response = await _client.PutAsJsonAsync($"/api/pet/{userId}/skin-color", new { color = newColor });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBackgroundColor_應該成功更新背景色()
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
        var response = await _client.PutAsJsonAsync($"/api/pet/{userId}/background-color", new { backgroundColor = newBackground });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}