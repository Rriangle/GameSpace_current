using System.Threading.Tasks;

namespace GameSpace.Services
{
    public interface IErrorTrackingService
    {
        Task TrackErrorAsync(Exception exception, string? userId = null, Dictionary<string, object>? properties = null);
        Task TrackEventAsync(string eventName, string? userId = null, Dictionary<string, object>? properties = null);
        Task TrackPerformanceAsync(string operationName, long durationMs, string? userId = null);
        Task<List<ErrorLog>> GetRecentErrorsAsync(int count = 50);
        Task<List<EventLog>> GetRecentEventsAsync(int count = 50);
    }

    public class ErrorLog
    {
        public int Id { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? RequestPath { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }

    public class EventLog
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? RequestPath { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}