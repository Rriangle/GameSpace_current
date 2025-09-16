using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;

namespace GameSpace.Tests.Services;

/// <summary>
/// UserService 單元測試
/// </summary>
public class UserServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly UserService _userService;
    private readonly Fixture _fixture;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _userService = new UserService(_context);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetUserByEmailAsync_當用戶存在時_應該返回用戶資訊()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            User_Name = "測試用戶",
            User_Email = email,
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
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.User_Email.Should().Be(email);
        result.User_Name.Should().Be("測試用戶");
    }

    [Fact]
    public async Task GetUserByEmailAsync_當用戶不存在時_應該返回null()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_當用戶存在時_應該返回用戶資訊()
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
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.User_ID.Should().Be(userId);
        result.User_Name.Should().Be("測試用戶");
    }

    [Fact]
    public async Task GetUserByIdAsync_當用戶不存在時_應該返回null()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_應該成功創建用戶()
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
        var result = await _userService.CreateUserAsync(
            userData.User_Name,
            userData.User_Email,
            userData.User_Password,
            userData.User_Phone,
            userData.User_Address
        );

        // Assert
        result.Should().NotBeNull();
        result.User_Name.Should().Be(userData.User_Name);
        result.User_Email.Should().Be(userData.User_Email);
        result.User_Phone.Should().Be(userData.User_Phone);
        result.User_Address.Should().Be(userData.User_Address);
        result.User_Status.Should().Be("Active");
        result.User_Register_Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateUserAsync_當郵箱已存在時_應該返回null()
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

        // Act
        var result = await _userService.CreateUserAsync(
            "新用戶",
            email,
            "password123",
            "0987654321",
            "新地址"
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_應該成功更新用戶資訊()
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
        var result = await _userService.UpdateUserAsync(
            userId,
            updateData.User_Name,
            updateData.User_Phone,
            updateData.User_Address
        );

        // Assert
        result.Should().BeTrue();
        
        var updatedUser = await _context.Users.FirstAsync(u => u.User_ID == userId);
        updatedUser.User_Name.Should().Be(updateData.User_Name);
        updatedUser.User_Phone.Should().Be(updateData.User_Phone);
        updatedUser.User_Address.Should().Be(updateData.User_Address);
    }

    [Fact]
    public async Task UpdateUserAsync_當用戶不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _userService.UpdateUserAsync(
            userId,
            "更新用戶",
            "0987654321",
            "更新地址"
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLastLoginTimeAsync_應該成功更新最後登入時間()
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
            User_Last_Login_Time = DateTime.UtcNow.AddDays(-1)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var originalLoginTime = user.User_Last_Login_Time;

        // Act
        var result = await _userService.UpdateLastLoginTimeAsync(userId);

        // Assert
        result.Should().BeTrue();
        
        var updatedUser = await _context.Users.FirstAsync(u => u.User_ID == userId);
        updatedUser.User_Last_Login_Time.Should().BeAfter(originalLoginTime);
    }

    [Fact]
    public async Task UpdateLastLoginTimeAsync_當用戶不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _userService.UpdateLastLoginTimeAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_當舊密碼正確時_應該成功更改密碼()
    {
        // Arrange
        var userId = 1;
        var oldPassword = "oldpassword";
        var newPassword = "newpassword";
        
        var user = new User
        {
            User_ID = userId,
            User_Name = "測試用戶",
            User_Email = "test@example.com",
            User_Password = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        result.Should().BeTrue();
        
        var updatedUser = await _context.Users.FirstAsync(u => u.User_ID == userId);
        BCrypt.Net.BCrypt.Verify(newPassword, updatedUser.User_Password).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_當舊密碼不正確時_應該返回false()
    {
        // Arrange
        var userId = 1;
        var oldPassword = "oldpassword";
        var wrongPassword = "wrongpassword";
        var newPassword = "newpassword";
        
        var user = new User
        {
            User_ID = userId,
            User_Name = "測試用戶",
            User_Email = "test@example.com",
            User_Password = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            User_Phone = "0912345678",
            User_Address = "測試地址",
            User_Status = "Active",
            User_Register_Time = DateTime.UtcNow,
            User_Last_Login_Time = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userService.ChangePasswordAsync(userId, wrongPassword, newPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_當用戶不存在時_應該返回false()
    {
        // Arrange
        var userId = 999;

        // Act
        var result = await _userService.ChangePasswordAsync(userId, "oldpassword", "newpassword");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}