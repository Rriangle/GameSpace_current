using System;
using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// 快取服務介面
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 取得快取資料
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="key">快取鍵值</param>
        /// <returns>快取資料</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// 設定快取資料
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="key">快取鍵值</param>
        /// <param name="value">快取資料</param>
        /// <param name="expiration">過期時間</param>
        /// <returns>是否成功</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// 移除快取資料
        /// </summary>
        /// <param name="key">快取鍵值</param>
        /// <returns>是否成功</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// 移除符合模式的快取資料
        /// </summary>
        /// <param name="pattern">模式</param>
        /// <returns>移除的數量</returns>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// 檢查快取是否存在
        /// </summary>
        /// <param name="key">快取鍵值</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 取得或設定快取資料
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="key">快取鍵值</param>
        /// <param name="factory">資料工廠方法</param>
        /// <param name="expiration">過期時間</param>
        /// <returns>快取資料</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
    }
}