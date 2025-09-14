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
/// UserController 整合測試
/// </summary>
public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly GameSpaceDbContext _context;

    public UserControllerTests(WebApplicationFactory<Program> factory)
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
    public async Task GetUser_當用戶存在時_應該返回用戶資訊()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            User_ID = userId,
            User_Name = "測試用戶",
            User_Email = "test@example.com",
            User_Password = "hashedpassword",
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userData.Should().NotBeNull();
        userData.User_ID.Should().Be(userId);
        userData.User_Name.Should().Be("測試用戶");
    }

    [Fact]
    public async Task GetUser_當用戶不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;

        // Act
        var response = await _client.GetAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_應該成功註冊新用戶()
    {
        // Arrange
        var userData = new
        {
            User_Name = "新用戶",
            User_Email = "newuser@example.com",
            User_Password = "password123",
            User_Phone = "0912345678",
            User_Address = "新地址"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/register", userData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
    }

    [Fact]
    public async Task Register_當郵箱已存在時_應該返回400()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User
        {
            User_Name = "現有用戶",
            User_Email = email,
            User_Password = "hashedpassword",
            User_Phone = "0912345678",
            User_Address = "現有地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var userData = new
        {
            User_Name = "新用戶",
            User_Email = email,
            User_Password = "password123",
            User_Phone = "0987654321",
            User_Address = "新地址"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/register", userData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_當憑證正確時_應該成功登入()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            User_Name = "測試用戶",
            User_Email = email,
            User_Password = hashedPassword,
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginData = new
        {
            User_Email = email,
            User_Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/login", loginData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result["success"].Should().Be(true);
        result.Should().ContainKey("token");
    }

    [Fact]
    public async Task Login_當憑證不正確時_應該返回401()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            User_Name = "測試用戶",
            User_Email = email,
            User_Password = hashedPassword,
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginData = new
        {
            User_Email = email,
            User_Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/login", loginData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_當用戶存在時_應該成功更新用戶資訊()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            User_ID = userId,
            User_Name = "原始用戶",
            User_Email = "original@example.com",
            User_Password = "hashedpassword",
            User_Phone = "0912345678",
            User_Address = "原始地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var updateData = new
        {
            User_Name = "更新用戶",
            User_Phone = "0987654321",
            User_Address = "更新地址"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}", updateData);

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
    public async Task UpdateUser_當用戶不存在時_應該返回404()
    {
        // Arrange
        var userId = 999;
        var updateData = new
        {
            User_Name = "更新用戶",
            User_Phone = "0987654321",
            User_Address = "更新地址"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}", updateData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_當舊密碼正確時_應該成功更改密碼()
    {
        // Arrange
        var userId = 1;
        var oldPassword = "oldpassword";
        var newPassword = "newpassword";
        var hashedOldPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword);
        
        var user = new User
        {
            User_ID = userId,
            User_Name = "測試用戶",
            User_Email = "test@example.com",
            User_Password = hashedOldPassword,
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var passwordData = new
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}/change-password", passwordData);

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
    public async Task ChangePassword_當舊密碼不正確時_應該返回400()
    {
        // Arrange
        var userId = 1;
        var oldPassword = "oldpassword";
        var wrongPassword = "wrongpassword";
        var newPassword = "newpassword";
        var hashedOldPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword);
        
        var user = new User
        {
            User_ID = userId,
            User_Name = "測試用戶",
            User_Email = "test@example.com",
            User_Password = hashedOldPassword,
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var passwordData = new
        {
            OldPassword = wrongPassword,
            NewPassword = newPassword
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}/change-password", passwordData);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}