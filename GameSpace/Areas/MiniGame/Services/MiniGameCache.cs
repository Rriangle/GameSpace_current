using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Area 專用輕量級記憶體快取服務
    /// 提供短期 TTL 快取（預設 5 分鐘）和快取失效管理
    /// </summary>
    public class MiniGameCache
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly ConcurrentHashSet<string> _trackedKeys = new();

        public MiniGameCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 取得或建立快取項目
        /// </summary>
        /// <typeparam name="T">快取值類型</typeparam>
        /// <param name="key">快取鍵</param>
        /// <param name="ttl">存活時間</param>
        /// <param name="factory">資料工廠函數</param>
        /// <param name="ct">取消權杖</param>
        /// <param name="bypass">是否略過快取</param>
        /// <returns>快取或新建的值</returns>
        public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct, bool bypass = false)
        {
            if (bypass)
            {
                // 略過快取，直接執行並更新
                var freshValue = await factory(ct);
                SetCacheValue(key, freshValue, ttl);
                return freshValue;
            }

            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }

            var newValue = await factory(ct);
            SetCacheValue(key, newValue, ttl);
            return newValue;
        }

        /// <summary>
        /// 設定快取值並追蹤鍵
        /// </summary>
        private void SetCacheValue<T>(string key, T value, TimeSpan ttl)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
                Priority = CacheItemPriority.Normal
            };

            options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = (k, v, r, s) => _trackedKeys.TryRemove(k.ToString()!)
            });

            _memoryCache.Set(key, value, options);
            _trackedKeys.Add(key);
        }

        /// <summary>
        /// 清除所有 MiniGame 相關快取
        /// </summary>
        public void InvalidateAll()
        {
            var keysToRemove = _trackedKeys.Where(k => k.StartsWith("MiniGame:")).ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _trackedKeys.TryRemove(key);
            }
        }

        /// <summary>
        /// 從 HTTP 請求建立標準化快取鍵
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <returns>標準化快取鍵</returns>
        public static string MakeKey(HttpRequest request)
        {
            var path = request.Path.Value ?? "";
            var normalizedQuery = NormalizeQueryString(request.Query);
            return $"MiniGame:{path}:{normalizedQuery}";
        }

        /// <summary>
        /// 標準化查詢字串（排序、修剪）
        /// </summary>
        /// <param name="query">查詢參數集合</param>
        /// <returns>標準化後的查詢字串</returns>
        public static string NormalizeQueryString(IQueryCollection query)
        {
            if (!query.Any())
                return "";

            var sortedParams = query
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => $"{kv.Key.Trim().ToLowerInvariant()}={kv.Value.ToString().Trim()}")
                .ToArray();

            return string.Join("&", sortedParams);
        }

        /// <summary>
        /// 取得快取統計資訊
        /// </summary>
        /// <returns>快取統計</returns>
        public object GetCacheStats()
        {
            var miniGameKeys = _trackedKeys.Where(k => k.StartsWith("MiniGame:")).ToList();
            
            return new
            {
                total_keys = _trackedKeys.Count,
                minigame_keys = miniGameKeys.Count,
                keys = miniGameKeys.Take(10).ToList(), // 最多顯示 10 個鍵
                timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 執行緒安全的 HashSet 實作
    /// </summary>
    public class ConcurrentHashSet<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary = new();

        public void Add(T item) => _dictionary.TryAdd(item, 0);

        public bool TryRemove(T item) => _dictionary.TryRemove(item, out _);

        public bool Contains(T item) => _dictionary.ContainsKey(item);

        public int Count => _dictionary.Count;

        public IEnumerable<T> Where(Func<T, bool> predicate) => _dictionary.Keys.Where(predicate);

        public List<T> ToList() => _dictionary.Keys.ToList();
    }
}