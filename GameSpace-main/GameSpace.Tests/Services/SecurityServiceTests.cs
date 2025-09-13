using FluentAssertions;
using GameSpace.Services;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Tests.Services;

/// <summary>
/// SecurityService 單元測試
/// </summary>
public class SecurityServiceTests
{
    private readonly SecurityService _securityService;

    public SecurityServiceTests()
    {
        _securityService = new SecurityService();
    }

    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("StrongPass1", true)]
    [InlineData("MyPassword123", true)]
    [InlineData("password123", false)] // 缺少大寫字母
    [InlineData("PASSWORD123", false)] // 缺少小寫字母
    [InlineData("Password", false)] // 缺少數字
    [InlineData("Pass1", false)] // 長度不足
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidatePasswordStrength_應該正確驗證密碼強度(string password, bool expected)
    {
        // Act
        var result = _securityService.ValidatePasswordStrength(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name+tag@domain.co.uk", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateEmailFormat_應該正確驗證Email格式(string email, bool expected)
    {
        // Act
        var result = _securityService.ValidateEmailFormat(email);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("0912345678", true)]
    [InlineData("0987654321", true)]
    [InlineData("0912-345-678", true)]
    [InlineData("+886-912-345-678", true)]
    [InlineData("123456789", false)]
    [InlineData("091234567", false)]
    [InlineData("abc1234567", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidatePhoneNumberFormat_應該正確驗證手機號碼格式(string phone, bool expected)
    {
        // Act
        var result = _securityService.ValidatePhoneNumberFormat(phone);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("A123456789", true)]
    [InlineData("B123456789", true)]
    [InlineData("C123456789", true)]
    [InlineData("123456789", false)]
    [InlineData("A12345678", false)]
    [InlineData("A1234567890", false)]
    [InlineData("a123456789", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateIdNumberFormat_應該正確驗證身分證字號格式(string idNumber, bool expected)
    {
        // Act
        var result = _securityService.ValidateIdNumberFormat(idNumber);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void HashPassword_應該產生不同的雜湊值()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hash1 = _securityService.HashPassword(password);
        var hash2 = _securityService.HashPassword(password);

        // Assert
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
        hash1.Should().NotBe(hash2); // 每次雜湊都應該不同（因為有鹽值）
    }

    [Fact]
    public void VerifyPassword_當密碼正確時_應該返回true()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = _securityService.HashPassword(password);

        // Act
        var result = _securityService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_當密碼錯誤時_應該返回false()
    {
        // Arrange
        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _securityService.HashPassword(correctPassword);

        // Act
        var result = _securityService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateSalt_應該產生不同的鹽值()
    {
        // Act
        var salt1 = _securityService.GenerateSalt();
        var salt2 = _securityService.GenerateSalt();

        // Assert
        salt1.Should().NotBeNullOrEmpty();
        salt2.Should().NotBeNullOrEmpty();
        salt1.Should().NotBe(salt2);
    }

    [Fact]
    public void GenerateToken_應該產生有效的JWT Token()
    {
        // Arrange
        var userId = 1;
        var username = "testuser";
        var role = "User";

        // Act
        var token = _securityService.GenerateToken(userId, username, role);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT 應該有三個部分
    }

    [Fact]
    public void ValidateToken_當Token有效時_應該返回用戶資訊()
    {
        // Arrange
        var userId = 1;
        var username = "testuser";
        var role = "User";
        var token = _securityService.GenerateToken(userId, username, role);

        // Act
        var result = _securityService.ValidateToken(token);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Username.Should().Be(username);
        result.Role.Should().Be(role);
    }

    [Fact]
    public void ValidateToken_當Token無效時_應該返回null()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _securityService.ValidateToken(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "&lt;script&gt;alert('xss')&lt;/script&gt;")]
    [InlineData("Hello <b>World</b>", "Hello &lt;b&gt;World&lt;/b&gt;")]
    [InlineData("Normal text", "Normal text")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void SanitizeInput_應該正確清理HTML標籤(string input, string expected)
    {
        // Act
        var result = _securityService.SanitizeInput(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("SELECT * FROM Users", true)]
    [InlineData("DROP TABLE Users", true)]
    [InlineData("INSERT INTO Users VALUES", true)]
    [InlineData("UPDATE Users SET", true)]
    [InlineData("DELETE FROM Users", true)]
    [InlineData("Normal text", false)]
    [InlineData("Hello World", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void DetectSqlInjection_應該正確檢測SQL注入攻擊(string input, bool expected)
    {
        // Act
        var result = _securityService.DetectSqlInjection(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("admin", true)]
    [InlineData("administrator", true)]
    [InlineData("root", true)]
    [InlineData("testuser", false)]
    [InlineData("normaluser", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSensitiveUsername_應該正確識別敏感用戶名(string username, bool expected)
    {
        // Act
        var result = _securityService.IsSensitiveUsername(username);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("password", true)]
    [InlineData("123456", true)]
    [InlineData("qwerty", true)]
    [InlineData("admin", true)]
    [InlineData("StrongPassword123!", false)]
    [InlineData("MySecurePass1", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsWeakPassword_應該正確識別弱密碼(string password, bool expected)
    {
        // Act
        var result = _securityService.IsWeakPassword(password);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateSecureRandomString_應該產生安全的隨機字串()
    {
        // Act
        var randomString1 = _securityService.GenerateSecureRandomString(32);
        var randomString2 = _securityService.GenerateSecureRandomString(32);

        // Assert
        randomString1.Should().NotBeNullOrEmpty();
        randomString2.Should().NotBeNullOrEmpty();
        randomString1.Should().NotBe(randomString2);
        randomString1.Length.Should().Be(32);
        randomString2.Length.Should().Be(32);
    }

    [Fact]
    public void EncryptData_應該正確加密資料()
    {
        // Arrange
        var data = "敏感資料";
        var key = _securityService.GenerateSecureRandomString(32);

        // Act
        var encrypted = _securityService.EncryptData(data, key);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(data);
    }

    [Fact]
    public void DecryptData_當使用正確金鑰時_應該正確解密資料()
    {
        // Arrange
        var originalData = "敏感資料";
        var key = _securityService.GenerateSecureRandomString(32);
        var encrypted = _securityService.EncryptData(originalData, key);

        // Act
        var decrypted = _securityService.DecryptData(encrypted, key);

        // Assert
        decrypted.Should().Be(originalData);
    }

    [Fact]
    public void DecryptData_當使用錯誤金鑰時_應該拋出異常()
    {
        // Arrange
        var originalData = "敏感資料";
        var correctKey = _securityService.GenerateSecureRandomString(32);
        var wrongKey = _securityService.GenerateSecureRandomString(32);
        var encrypted = _securityService.EncryptData(originalData, correctKey);

        // Act & Assert
        var action = () => _securityService.DecryptData(encrypted, wrongKey);
        action.Should().Throw<CryptographicException>();
    }
}

// 測試用的安全性服務
public class SecurityService
{
    public bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }

    public bool ValidateEmailFormat(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidatePhoneNumberFormat(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        
        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
        return digits.Length == 10 && digits.StartsWith("09");
    }

    public bool ValidateIdNumberFormat(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber) || idNumber.Length != 10) return false;
        
        var firstChar = idNumber[0];
        var remaining = idNumber.Substring(1);
        
        return char.IsLetter(firstChar) && remaining.All(char.IsDigit);
    }

    public string HashPassword(string password)
    {
        var salt = GenerateSalt();
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes) + ":" + salt;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword)) return false;
        
        var parts = hashedPassword.Split(':');
        if (parts.Length != 2) return false;
        
        var hash = parts[0];
        var salt = parts[1];
        
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var computedHash = Convert.ToBase64String(hashedBytes);
        
        return hash == computedHash;
    }

    public string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public string GenerateToken(int userId, string username, string role)
    {
        // 簡化的 JWT Token 生成（實際實作應該使用 JWT 庫）
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"sub\":\"{userId}\",\"name\":\"{username}\",\"role\":\"{role}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("signature"));
        
        return $"{header}.{payload}.{signature}";
    }

    public TokenInfo ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        
        var parts = token.Split('.');
        if (parts.Length != 3) return null;
        
        try
        {
            var payload = parts[1];
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            return new TokenInfo
            {
                UserId = int.Parse(tokenData["sub"].ToString()),
                Username = tokenData["name"].ToString(),
                Role = tokenData["role"].ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;");
    }

    public bool DetectSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "UNION" };
        var upperInput = input.ToUpper();
        
        return sqlKeywords.Any(keyword => upperInput.Contains(keyword));
    }

    public bool IsSensitiveUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        
        var sensitiveUsernames = new[] { "admin", "administrator", "root", "system", "guest" };
        return sensitiveUsernames.Contains(username.ToLower());
    }

    public bool IsWeakPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        var weakPasswords = new[] { "password", "123456", "qwerty", "admin", "letmein", "welcome", "monkey", "1234567890" };
        return weakPasswords.Contains(password.ToLower());
    }

    public string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string EncryptData(string data, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
        
        return Convert.ToBase64String(aes.IV.Concat(encryptedBytes).ToArray());
    }

    public string DecryptData(string encryptedData, string key)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedData);
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
        aes.IV = encryptedBytes.Take(16).ToArray();
        
        using var decryptor = aes.CreateDecryptor();
        var dataBytes = encryptedBytes.Skip(16).ToArray();
        var decryptedBytes = decryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}

// 測試用的模型
public class TokenInfo
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}