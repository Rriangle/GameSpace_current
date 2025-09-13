using GameSpace.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Diagnostics;
using System.Runtime;

namespace GameSpace.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly GameSpaceDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(
            GameSpaceDbContext context,
            IMemoryCache cache,
            IConnectionMultiplexer? redis,
            ILogger<HealthCheckService> logger)
        {
            _context = context;
            _cache = cache;
            _redis = redis;
            _logger = logger;
        }

        public async Task<HealthStatus> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // 測試資料庫連線
                await _context.Database.CanConnectAsync();
                
                // 執行簡單查詢
                var userCount = await _context.Users.CountAsync();
                
                stopwatch.Stop();
                
                return new HealthStatus
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Message = $"Database connection successful. Total users: {userCount}",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database health check failed");
                
                return new HealthStatus
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Message = $"Database connection failed: {ex.Message}",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<HealthStatus> CheckRedisHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (_redis == null)
                {
                    return new HealthStatus
                    {
                        IsHealthy = true,
                        Status = "Not Configured",
                        Message = "Redis is not configured, using in-memory cache",
                        ResponseTimeMs = 0
                    };
                }

                var database = _redis.GetDatabase();
                await database.PingAsync();
                
                stopwatch.Stop();
                
                return new HealthStatus
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Message = "Redis connection successful",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Redis health check failed");
                
                return new HealthStatus
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Message = $"Redis connection failed: {ex.Message}",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<HealthStatus> CheckOverallHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var dbHealth = await CheckDatabaseHealthAsync();
                var redisHealth = await CheckRedisHealthAsync();
                
                stopwatch.Stop();
                
                var isHealthy = dbHealth.IsHealthy && redisHealth.IsHealthy;
                var status = isHealthy ? "Healthy" : "Unhealthy";
                var message = $"Database: {dbHealth.Status}, Cache: {redisHealth.Status}";
                
                return new HealthStatus
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Message = message,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Overall health check failed");
                
                return new HealthStatus
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Message = $"Health check failed: {ex.Message}",
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            try
            {
                var metrics = new SystemMetrics();
                
                // 資料庫統計
                metrics.TotalUsers = await _context.Users.CountAsync();
                metrics.TotalPets = await _context.Pets.CountAsync();
                metrics.TotalForums = await _context.Forums.CountAsync();
                metrics.TotalOrders = await _context.OrderInfos.CountAsync();
                metrics.TotalProducts = await _context.ProductInfos.CountAsync();
                
                // 活躍用戶（最近7天有活動的用戶）
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                metrics.ActiveUsers = await _context.Users
                    .Where(u => u.CreatedAt >= sevenDaysAgo)
                    .CountAsync();
                
                // 系統資源使用情況
                var process = Process.GetCurrentProcess();
                metrics.MemoryUsageMB = process.WorkingSet64 / 1024.0 / 1024.0;
                metrics.CpuUsagePercent = GetCpuUsage();
                
                // 請求統計（從快取中獲取）
                var requestCount = _cache.Get<long>("RequestCount");
                var totalResponseTime = _cache.Get<long>("TotalResponseTime");
                metrics.RequestCount = requestCount;
                metrics.AverageResponseTimeMs = requestCount > 0 ? (double)totalResponseTime / requestCount : 0;
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect system metrics");
                return new SystemMetrics();
            }
        }

        private double GetCpuUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;
                
                Thread.Sleep(100); // 等待100ms
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                
                return Math.Min(100, cpuUsageTotal * 100);
            }
            catch
            {
                return 0;
            }
        }
    }
}