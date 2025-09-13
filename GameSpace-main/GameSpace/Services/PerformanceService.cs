using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace GameSpace.Services
{
    /// <summary>
    /// 效能監控服務實作
    /// </summary>
    public class PerformanceService : IPerformanceService
    {
        private readonly ILogger<PerformanceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentQueue<ApiResponseMetric> _apiMetrics;
        private readonly ConcurrentQueue<DatabaseQueryMetric> _dbMetrics;
        private readonly ConcurrentQueue<CacheHitMetric> _cacheMetrics;
        private readonly PerformanceStats _stats;
        private readonly object _statsLock = new object();

        public PerformanceService(ILogger<PerformanceService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiMetrics = new ConcurrentQueue<ApiResponseMetric>();
            _dbMetrics = new ConcurrentQueue<DatabaseQueryMetric>();
            _cacheMetrics = new ConcurrentQueue<CacheHitMetric>();
            _stats = new PerformanceStats();
        }

        public async Task LogApiResponseTimeAsync(string endpoint, string method, long responseTime, int statusCode)
        {
            var metric = new ApiResponseMetric
            {
                Endpoint = endpoint,
                Method = method,
                ResponseTime = responseTime,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };

            _apiMetrics.Enqueue(metric);

            // 保持最近 1000 筆記錄
            while (_apiMetrics.Count > 1000)
            {
                _apiMetrics.TryDequeue(out _);
            }

            _logger.LogDebug("API 響應時間: {Endpoint} {Method} {ResponseTime}ms {StatusCode}", 
                endpoint, method, responseTime, statusCode);

            await Task.CompletedTask;
        }

        public async Task LogDatabaseQueryTimeAsync(string query, long executionTime, int recordCount)
        {
            var metric = new DatabaseQueryMetric
            {
                Query = query,
                ExecutionTime = executionTime,
                RecordCount = recordCount,
                Timestamp = DateTime.UtcNow
            };

            _dbMetrics.Enqueue(metric);

            // 保持最近 1000 筆記錄
            while (_dbMetrics.Count > 1000)
            {
                _dbMetrics.TryDequeue(out _);
            }

            if (executionTime > 1000) // 超過 1 秒的慢查詢
            {
                _logger.LogWarning("慢查詢警告: {Query} {ExecutionTime}ms {RecordCount} records", 
                    query, executionTime, recordCount);
            }

            await Task.CompletedTask;
        }

        public async Task LogCacheHitAsync(string cacheKey, bool hit)
        {
            var metric = new CacheHitMetric
            {
                CacheKey = cacheKey,
                Hit = hit,
                Timestamp = DateTime.UtcNow
            };

            _cacheMetrics.Enqueue(metric);

            // 保持最近 1000 筆記錄
            while (_cacheMetrics.Count > 1000)
            {
                _cacheMetrics.TryDequeue(out _);
            }

            _logger.LogDebug("快取 {Status}: {CacheKey}", hit ? "命中" : "未命中", cacheKey);

            await Task.CompletedTask;
        }

        public async Task LogMemoryUsageAsync()
        {
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64;

            _logger.LogDebug("記憶體使用量: {MemoryUsage} MB", memoryUsage / 1024 / 1024);

            lock (_statsLock)
            {
                _stats.MemoryUsageBytes = memoryUsage;
                _stats.LastUpdated = DateTime.UtcNow;
            }

            await Task.CompletedTask;
        }

        public async Task LogCpuUsageAsync()
        {
            var process = Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds;

            _logger.LogDebug("CPU 使用時間: {CpuUsage} ms", cpuUsage);

            await Task.CompletedTask;
        }

        public async Task<PerformanceStats> GetPerformanceStatsAsync()
        {
            lock (_statsLock)
            {
                // 計算 API 平均響應時間
                var apiMetrics = _apiMetrics.ToArray();
                if (apiMetrics.Length > 0)
                {
                    _stats.AverageApiResponseTime = apiMetrics.Average(m => m.ResponseTime);
                    _stats.TotalRequests = apiMetrics.Length;
                    _stats.ErrorCount = apiMetrics.Count(m => m.StatusCode >= 400);
                }

                // 計算資料庫平均查詢時間
                var dbMetrics = _dbMetrics.ToArray();
                if (dbMetrics.Length > 0)
                {
                    _stats.AverageDatabaseQueryTime = dbMetrics.Average(m => m.ExecutionTime);
                }

                // 計算快取命中率
                var cacheMetrics = _cacheMetrics.ToArray();
                if (cacheMetrics.Length > 0)
                {
                    var hits = cacheMetrics.Count(m => m.Hit);
                    _stats.CacheHitRate = (double)hits / cacheMetrics.Length * 100;
                }

                return _stats;
            }
        }

        public async Task<HealthStatus> CheckHealthAsync()
        {
            var issues = new List<string>();
            var isHealthy = true;

            // 檢查記憶體使用量
            var process = Process.GetCurrentProcess();
            var memoryUsageMB = process.WorkingSet64 / 1024 / 1024;
            var maxMemoryMB = _configuration.GetValue<int>("Performance:MaxMemoryMB", 1024);

            if (memoryUsageMB > maxMemoryMB)
            {
                issues.Add($"記憶體使用量過高: {memoryUsageMB}MB > {maxMemoryMB}MB");
                isHealthy = false;
            }

            // 檢查 API 響應時間
            var stats = await GetPerformanceStatsAsync();
            var maxResponseTime = _configuration.GetValue<int>("Performance:MaxApiResponseTimeMs", 5000);

            if (stats.AverageApiResponseTime > maxResponseTime)
            {
                issues.Add($"API 響應時間過長: {stats.AverageApiResponseTime:F2}ms > {maxResponseTime}ms");
                isHealthy = false;
            }

            // 檢查錯誤率
            var errorRate = stats.TotalRequests > 0 ? (double)stats.ErrorCount / stats.TotalRequests * 100 : 0;
            var maxErrorRate = _configuration.GetValue<double>("Performance:MaxErrorRate", 5.0);

            if (errorRate > maxErrorRate)
            {
                issues.Add($"錯誤率過高: {errorRate:F2}% > {maxErrorRate}%");
                isHealthy = false;
            }

            return new HealthStatus
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "健康" : "異常",
                Issues = issues.ToArray(),
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// API 響應指標
    /// </summary>
    internal class ApiResponseMetric
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 資料庫查詢指標
    /// </summary>
    internal class DatabaseQueryMetric
    {
        public string Query { get; set; } = string.Empty;
        public long ExecutionTime { get; set; }
        public int RecordCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 快取命中指標
    /// </summary>
    internal class CacheHitMetric
    {
        public string CacheKey { get; set; } = string.Empty;
        public bool Hit { get; set; }
        public DateTime Timestamp { get; set; }
    }
}