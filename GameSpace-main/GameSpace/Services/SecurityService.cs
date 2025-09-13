using GameSpace.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// 安全性服務實作
    /// </summary>
    public class SecurityService : ISecurityService
    {
        private readonly GameSpaceDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SecurityService> _logger;

        // 密碼強度規則
        private static readonly Regex PasswordPattern = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            RegexOptions.Compiled);

        // 危險字元模式
        private static readonly Regex[] DangerousPatterns = {
            new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"union\s+select", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"drop\s+table", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"insert\s+into", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"delete\s+from", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"update\s+set", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"exec\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public SecurityService(GameSpaceDbContext context, IMemoryCache cache, ILogger<SecurityService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> ValidatePasswordStrengthAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // 檢查長度
            if (password.Length < 8)
                return false;

            // 檢查複雜度
            if (!PasswordPattern.IsMatch(password))
                return false;

            // 檢查常見弱密碼
            var commonPasswords = new[] { "password", "123456", "qwerty", "abc123", "password123" };
            if (commonPasswords.Any(common => password.ToLower().Contains(common)))
                return false;

            return await Task.FromResult(true);
        }

        public async Task<bool> IsAccountLockedAsync(string userAccount)
        {
            var cacheKey = $"account_lock_{userAccount}";
            if (_cache.TryGetValue(cacheKey, out bool isLocked))
            {
                return isLocked;
            }

            // 檢查資料庫中的鎖定狀態
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserAccount == userAccount);

            if (user == null)
                return false;

            // 檢查是否超過最大失敗次數
            var failedCount = await _context.UserTokens
                .Where(t => t.UserId == user.UserId && t.TokenType == "FailedLogin")
                .CountAsync();

            var maxFailedAttempts = 5; // 可配置
            var lockoutDuration = TimeSpan.FromMinutes(15); // 可配置

            if (failedCount >= maxFailedAttempts)
            {
                // 檢查鎖定是否已過期
                var lastFailedLogin = await _context.UserTokens
                    .Where(t => t.UserId == user.UserId && t.TokenType == "FailedLogin")
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastFailedLogin != null && 
                    DateTime.UtcNow - lastFailedLogin.CreatedAt < lockoutDuration)
                {
                    _cache.Set(cacheKey, true, lockoutDuration);
                    return true;
                }
            }

            return false;
        }

        public async Task RecordFailedLoginAsync(string userAccount, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserAccount == userAccount);

            if (user == null)
                return;

            // 記錄失敗登入
            var failedLogin = new UserToken
            {
                UserId = user.UserId,
                TokenType = "FailedLogin",
                TokenValue = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.UserTokens.Add(failedLogin);
            await _context.SaveChangesAsync();

            // 記錄安全事件
            await LogSecurityEventAsync("FailedLogin", user.UserId.ToString(), 
                $"Failed login attempt from {ipAddress}", ipAddress);

            _logger.LogWarning("記錄登入失敗: {UserAccount} from {IP}", userAccount, ipAddress);
        }

        public async Task ResetFailedLoginCountAsync(string userAccount)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserAccount == userAccount);

            if (user == null)
                return;

            // 刪除失敗登入記錄
            var failedLogins = await _context.UserTokens
                .Where(t => t.UserId == user.UserId && t.TokenType == "FailedLogin")
                .ToListAsync();

            _context.UserTokens.RemoveRange(failedLogins);
            await _context.SaveChangesAsync();

            // 清除快取
            var cacheKey = $"account_lock_{userAccount}";
            _cache.Remove(cacheKey);

            _logger.LogInformation("重置登入失敗計數: {UserAccount}", userAccount);
        }

        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            var cacheKey = $"blacklist_{ipAddress}";
            if (_cache.TryGetValue(cacheKey, out bool isBlacklisted))
            {
                return isBlacklisted;
            }

            // 這裡可以實作 IP 黑名單檢查
            // 例如從資料庫或外部服務檢查
            var isBlacklistedResult = false; // 實作黑名單檢查邏輯

            _cache.Set(cacheKey, isBlacklistedResult, TimeSpan.FromMinutes(30));
            return await Task.FromResult(isBlacklistedResult);
        }

        public async Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress)
        {
            try
            {
                // 這裡可以實作安全事件記錄
                // 例如寫入專門的安全日誌表或外部日誌系統
                _logger.LogWarning("安全事件: {EventType} - User: {UserId} - Details: {Details} - IP: {IP}", 
                    eventType, userId, details, ipAddress);

                // 可以實作更詳細的安全事件記錄
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄安全事件失敗: {EventType}", eventType);
            }
        }

        public async Task<bool> ValidateInputSafetyAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            foreach (var pattern in DangerousPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    _logger.LogWarning("檢測到危險輸入內容: {Pattern}", pattern.ToString());
                    return false;
                }
            }

            return await Task.FromResult(true);
        }

        public async Task<string> SanitizeHtmlAsync(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // 基本的 HTML 清理
            var sanitized = html;

            // 移除危險標籤
            var dangerousTags = new[] { "script", "iframe", "object", "embed", "form", "input" };
            foreach (var tag in dangerousTags)
            {
                var pattern = $@"<{tag}[^>]*>.*?</{tag}>";
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            // 移除危險屬性
            var dangerousAttributes = new[] { "onclick", "onload", "onerror", "onmouseover", "onfocus", "onblur" };
            foreach (var attr in dangerousAttributes)
            {
                var pattern = $@"\s{attr}\s*=\s*[""'][^""']*[""']";
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase);
            }

            // 編碼 HTML 實體
            sanitized = System.Net.WebUtility.HtmlEncode(sanitized);

            return await Task.FromResult(sanitized);
        }
    }
}