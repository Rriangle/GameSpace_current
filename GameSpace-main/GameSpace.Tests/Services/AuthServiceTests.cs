using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Models;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Tests.Services;

/// <summary>
/// AuthService 單元測試
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly GameSpaceDbContext _context;
    private readonly AuthService _authService;
    private readonly Fixture _fixture;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameSpaceDbContext(options);
        _authService = new AuthService(_context);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task RegisterAsync_當提供有效資料時_應該成功註冊用戶()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            User_Account = "testuser",
            User_Password = "Password123!",
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20)
        };

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("註冊成功");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Account == "testuser");
        user.Should().NotBeNull();
        user.User_Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task RegisterAsync_當帳號已存在時_應該返回錯誤()
    {
        // Arrange
        var existingUser = new User
        {
            User_Account = "existinguser",
            User_Password = "hashedpassword",
            User_Email = "existing@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "現有用戶",
            User_BirthDate = DateTime.Now.AddYears(-20)
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var registerRequest = new RegisterRequest
        {
            User_Account = "existinguser",
            User_Password = "Password123!",
            User_Email = "new@example.com",
            User_Phone = "0987654321",
            User_IDNumber = "B987654321",
            User_Nickname = "新用戶",
            User_BirthDate = DateTime.Now.AddYears(-20)
        };

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("帳號已存在");
    }

    [Fact]
    public async Task LoginAsync_當提供正確憑證時_應該成功登入()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = HashPassword(password);
        
        var user = new User
        {
            User_Account = "testuser",
            User_Password = hashedPassword,
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            User_Account = "testuser",
            User_Password = password
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.User_ID);
    }

    [Fact]
    public async Task LoginAsync_當密碼錯誤時_應該返回錯誤()
    {
        // Arrange
        var hashedPassword = HashPassword("CorrectPassword123!");
        
        var user = new User
        {
            User_Account = "testuser",
            User_Password = hashedPassword,
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            User_Account = "testuser",
            User_Password = "WrongPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("密碼錯誤");
    }

    [Fact]
    public async Task LoginAsync_當帳號被鎖定時_應該返回錯誤()
    {
        // Arrange
        var hashedPassword = HashPassword("Password123!");
        
        var user = new User
        {
            User_Account = "lockeduser",
            User_Password = hashedPassword,
            User_Email = "locked@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "鎖定用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 0,
            User_LockoutEnabled = true,
            User_LockoutEnd = DateTime.Now.AddMinutes(30)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            User_Account = "lockeduser",
            User_Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("帳號已鎖定");
    }

    [Fact]
    public async Task ValidateTokenAsync_當提供有效Token時_應該返回用戶資訊()
    {
        // Arrange
        var user = new User
        {
            User_Account = "testuser",
            User_Password = "hashedpassword",
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);

        // Act
        var result = await _authService.ValidateTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.User_ID);
        result.Username.Should().Be(user.User_Account);
    }

    [Fact]
    public async Task ValidateTokenAsync_當提供無效Token時_應該返回null()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_當提供有效RefreshToken時_應該返回新Token()
    {
        // Arrange
        var user = new User
        {
            User_Account = "testuser",
            User_Password = "hashedpassword",
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20),
            User_Status = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var refreshToken = _authService.GenerateRefreshToken();

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
        return Convert.ToBase64String(hashedBytes);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// 測試用的請求模型
public class RegisterRequest
{
    public string User_Account { get; set; } = string.Empty;
    public string User_Password { get; set; } = string.Empty;
    public string User_Email { get; set; } = string.Empty;
    public string User_Phone { get; set; } = string.Empty;
    public string User_IDNumber { get; set; } = string.Empty;
    public string User_Nickname { get; set; } = string.Empty;
    public DateTime User_BirthDate { get; set; }
}

public class LoginRequest
{
    public string User_Account { get; set; } = string.Empty;
    public string User_Password { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}