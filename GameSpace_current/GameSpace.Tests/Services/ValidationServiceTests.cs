using FluentAssertions;
using GameSpace.Services;

namespace GameSpace.Tests.Services;

/// <summary>
/// ValidationService 單元測試
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name+tag@domain.co.uk", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_應該正確驗證Email格式(string email, bool expected)
    {
        // Act
        var result = _validationService.IsValidEmail(email);

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
    public void IsValidPhoneNumber_應該正確驗證手機號碼格式(string phone, bool expected)
    {
        // Act
        var result = _validationService.IsValidPhoneNumber(phone);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("A123456789", true)]
    [InlineData("B123456789", true)]
    [InlineData("C123456789", true)]
    [InlineData("D123456789", true)]
    [InlineData("E123456789", true)]
    [InlineData("F123456789", true)]
    [InlineData("G123456789", true)]
    [InlineData("H123456789", true)]
    [InlineData("I123456789", true)]
    [InlineData("J123456789", true)]
    [InlineData("K123456789", true)]
    [InlineData("L123456789", true)]
    [InlineData("M123456789", true)]
    [InlineData("N123456789", true)]
    [InlineData("O123456789", true)]
    [InlineData("P123456789", true)]
    [InlineData("Q123456789", true)]
    [InlineData("R123456789", true)]
    [InlineData("S123456789", true)]
    [InlineData("T123456789", true)]
    [InlineData("U123456789", true)]
    [InlineData("V123456789", true)]
    [InlineData("W123456789", true)]
    [InlineData("X123456789", true)]
    [InlineData("Y123456789", true)]
    [InlineData("Z123456789", true)]
    [InlineData("123456789", false)]
    [InlineData("A12345678", false)]
    [InlineData("A1234567890", false)]
    [InlineData("a123456789", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidIdNumber_應該正確驗證身分證字號格式(string idNumber, bool expected)
    {
        // Act
        var result = _validationService.IsValidIdNumber(idNumber);

        // Assert
        result.Should().Be(expected);
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
    public void IsValidPassword_應該正確驗證密碼強度(string password, bool expected)
    {
        // Act
        var result = _validationService.IsValidPassword(password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("testuser", true)]
    [InlineData("user123", true)]
    [InlineData("test_user", true)]
    [InlineData("user-test", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("a", false)] // 長度不足
    [InlineData("thisusernameistoolongandshouldnotbevalid", false)] // 長度過長
    [InlineData("user@name", false)] // 包含特殊字符
    [InlineData("user name", false)] // 包含空格
    public void IsValidUsername_應該正確驗證用戶名格式(string username, bool expected)
    {
        // Act
        var result = _validationService.IsValidUsername(username);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("測試用戶", true)]
    [InlineData("User123", true)]
    [InlineData("用戶_測試", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("a", false)] // 長度不足
    [InlineData("thisnicknameistoolongandshouldnotbevalid", false)] // 長度過長
    public void IsValidNickname_應該正確驗證暱稱格式(string nickname, bool expected)
    {
        // Act
        var result = _validationService.IsValidNickname(nickname);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(18, true)]
    [InlineData(25, true)]
    [InlineData(65, true)]
    [InlineData(17, false)] // 未成年
    [InlineData(0, false)] // 無效年齡
    [InlineData(-5, false)] // 負數年齡
    [InlineData(150, false)] // 年齡過大
    public void IsValidAge_應該正確驗證年齡範圍(int age, bool expected)
    {
        // Act
        var result = _validationService.IsValidAge(age);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("#FF0000", true)]
    [InlineData("#00FF00", true)]
    [InlineData("#0000FF", true)]
    [InlineData("#123456", true)]
    [InlineData("#ABCDEF", true)]
    [InlineData("#abcdef", true)]
    [InlineData("FF0000", false)] // 缺少 #
    [InlineData("#GG0000", false)] // 無效字符
    [InlineData("#12345", false)] // 長度不足
    [InlineData("#1234567", false)] // 長度過長
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidHexColor_應該正確驗證十六進制顏色格式(string color, bool expected)
    {
        // Act
        var result = _validationService.IsValidHexColor(color);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("正常文章標題", true)]
    [InlineData("測試標題123", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("a", false)] // 長度不足
    [InlineData("thisistoolongtitlethatshouldnotbevalid", false)] // 長度過長
    public void IsValidTitle_應該正確驗證標題格式(string title, bool expected)
    {
        // Act
        var result = _validationService.IsValidTitle(title);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("這是一篇正常的文章內容", true)]
    [InlineData("測試內容123", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("a", false)] // 長度不足
    public void IsValidContent_應該正確驗證內容格式(string content, bool expected)
    {
        // Act
        var result = _validationService.IsValidContent(content);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://www.example.com/path", true)]
    [InlineData("ftp://example.com", false)] // 不支援的協議
    [InlineData("example.com", false)] // 缺少協議
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidUrl_應該正確驗證URL格式(string url, bool expected)
    {
        // Act
        var result = _validationService.IsValidUrl(url);

        // Assert
        result.Should().Be(expected);
    }
}

// 測試用的驗證服務
public class ValidationService
{
    public bool IsValidEmail(string email)
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

    public bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        
        // 移除所有非數字字符
        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
        
        // 檢查是否為10位數（台灣手機號碼）
        return digits.Length == 10 && digits.StartsWith("09");
    }

    public bool IsValidIdNumber(string idNumber)
    {
        if (string.IsNullOrEmpty(idNumber) || idNumber.Length != 10) return false;
        
        var firstChar = idNumber[0];
        var remaining = idNumber.Substring(1);
        
        // 檢查第一個字符是否為英文字母
        if (!char.IsLetter(firstChar)) return false;
        
        // 檢查其餘字符是否為數字
        return remaining.All(char.IsDigit);
    }

    public bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        // 至少8個字符，包含大小寫字母和數字
        return password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }

    public bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        
        // 3-20個字符，只能包含字母、數字、下劃線和連字符
        return username.Length >= 3 && username.Length <= 20 &&
               username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }

    public bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;
        
        // 2-20個字符
        return nickname.Length >= 2 && nickname.Length <= 20;
    }

    public bool IsValidAge(int age)
    {
        return age >= 18 && age <= 120;
    }

    public bool IsValidHexColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return false;
        
        return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
    }

    public bool IsValidTitle(string title)
    {
        if (string.IsNullOrEmpty(title)) return false;
        
        return title.Length >= 2 && title.Length <= 50;
    }

    public bool IsValidContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return false;
        
        return content.Length >= 10;
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == "http" || result.Scheme == "https");
    }
}