using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// 記憶體快取服務實作
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                _memoryCache.TryGetValue(key, out T? value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得快取資料失敗: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.Normal
                };

                _memoryCache.Set(key, value, options);
                _logger.LogDebug("設定快取資料成功: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定快取資料失敗: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("移除快取資料成功: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除快取資料失敗: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // 記憶體快取不支援模式匹配，這裡實作基本功能
            _logger.LogWarning("記憶體快取不支援模式匹配移除: {Pattern}", pattern);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查快取存在失敗: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
                {
                    return cachedValue;
                }

                var value = await factory();
                if (value != null)
                {
                    await SetAsync(key, value, expiration);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得或設定快取資料失敗: {Key}", key);
                return await factory();
            }
        }
    }
}