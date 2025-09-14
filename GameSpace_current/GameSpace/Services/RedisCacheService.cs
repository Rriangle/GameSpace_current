using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// Redis 快取服務實作
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 Redis 快取資料失敗: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.StringSetAsync(key, json, expiration);
                _logger.LogDebug("設定 Redis 快取資料成功: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定 Redis 快取資料失敗: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("移除 Redis 快取資料成功: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除 Redis 快取資料失敗: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints()[0]);
                var keys = server.Keys(pattern: pattern);
                
                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
                
                _logger.LogDebug("移除 Redis 快取資料成功: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除 Redis 快取資料失敗: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查 Redis 快取存在失敗: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
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
                _logger.LogError(ex, "取得或設定 Redis 快取資料失敗: {Key}", key);
                return await factory();
            }
        }
    }
}