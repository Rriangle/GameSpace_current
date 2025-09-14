using System.Threading.Tasks;

namespace GameSpace.Services
{
    public interface IHealthCheckService
    {
        Task<HealthStatus> CheckDatabaseHealthAsync();
        Task<HealthStatus> CheckRedisHealthAsync();
        Task<HealthStatus> CheckOverallHealthAsync();
        Task<SystemMetrics> GetSystemMetricsAsync();
    }

    public class HealthStatus
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class SystemMetrics
    {
        public long TotalUsers { get; set; }
        public long ActiveUsers { get; set; }
        public long TotalPets { get; set; }
        public long TotalForums { get; set; }
        public long TotalOrders { get; set; }
        public long TotalProducts { get; set; }
        public double MemoryUsageMB { get; set; }
        public double CpuUsagePercent { get; set; }
        public long RequestCount { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    }
}