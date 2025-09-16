using System;
using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// 效能監控服務介面
    /// </summary>
    public interface IPerformanceService
    {
        /// <summary>
        /// 記錄 API 響應時間
        /// </summary>
        /// <param name="endpoint">API 端點</param>
        /// <param name="method">HTTP 方法</param>
        /// <param name="responseTime">響應時間（毫秒）</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        Task LogApiResponseTimeAsync(string endpoint, string method, long responseTime, int statusCode);

        /// <summary>
        /// 記錄資料庫查詢時間
        /// </summary>
        /// <param name="query">查詢描述</param>
        /// <param name="executionTime">執行時間（毫秒）</param>
        /// <param name="recordCount">記錄數量</param>
        Task LogDatabaseQueryTimeAsync(string query, long executionTime, int recordCount);

        /// <summary>
        /// 記錄快取命中率
        /// </summary>
        /// <param name="cacheKey">快取鍵值</param>
        /// <param name="hit">是否命中</param>
        Task LogCacheHitAsync(string cacheKey, bool hit);

        /// <summary>
        /// 記錄記憶體使用量
        /// </summary>
        Task LogMemoryUsageAsync();

        /// <summary>
        /// 記錄 CPU 使用率
        /// </summary>
        Task LogCpuUsageAsync();

        /// <summary>
        /// 取得效能統計
        /// </summary>
        /// <returns>效能統計資料</returns>
        Task<PerformanceStats> GetPerformanceStatsAsync();

        /// <summary>
        /// 檢查系統健康狀態
        /// </summary>
        /// <returns>健康狀態</returns>
        Task<HealthStatus> CheckHealthAsync();
    }

    /// <summary>
    /// 效能統計資料
    /// </summary>
    public class PerformanceStats
    {
        public double AverageApiResponseTime { get; set; }
        public double AverageDatabaseQueryTime { get; set; }
        public double CacheHitRate { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double CpuUsagePercent { get; set; }
        public int TotalRequests { get; set; }
        public int ErrorCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}