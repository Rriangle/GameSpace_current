using GameSpace.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GameSpace.Services
{
    public class ErrorTrackingService : IErrorTrackingService
    {
        private readonly GameSpaceDbContext _context;
        private readonly ILogger<ErrorTrackingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ErrorTrackingService(
            GameSpaceDbContext context,
            ILogger<ErrorTrackingService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task TrackErrorAsync(Exception exception, string? userId = null, Dictionary<string, object>? properties = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var errorLog = new ErrorLog
                {
                    ExceptionType = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace ?? string.Empty,
                    UserId = userId,
                    RequestPath = httpContext?.Request.Path.Value,
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                    Properties = properties ?? new Dictionary<string, object>(),
                    OccurredAt = DateTime.UtcNow
                };

                // 記錄到資料庫
                await _context.ErrorLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();

                // 記錄到 Serilog
                _logger.LogError(exception, 
                    "Error tracked: {ExceptionType} - {Message} for user {UserId} at {RequestPath}",
                    errorLog.ExceptionType, errorLog.Message, userId, errorLog.RequestPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track error: {OriginalException}", exception.Message);
            }
        }

        public async Task TrackEventAsync(string eventName, string? userId = null, Dictionary<string, object>? properties = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var eventLog = new EventLog
                {
                    EventName = eventName,
                    UserId = userId,
                    RequestPath = httpContext?.Request.Path.Value,
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                    Properties = properties ?? new Dictionary<string, object>(),
                    OccurredAt = DateTime.UtcNow
                };

                // 記錄到資料庫
                await _context.EventLogs.AddAsync(eventLog);
                await _context.SaveChangesAsync();

                // 記錄到 Serilog
                _logger.LogInformation("Event tracked: {EventName} for user {UserId} at {RequestPath}",
                    eventName, userId, httpContext?.Request.Path.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track event: {EventName}", eventName);
            }
        }

        public async Task TrackPerformanceAsync(string operationName, long durationMs, string? userId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var properties = new Dictionary<string, object>
                {
                    ["DurationMs"] = durationMs,
                    ["OperationName"] = operationName
                };

                await TrackEventAsync("Performance", userId, properties);

                // 記錄到 Serilog
                _logger.LogInformation("Performance tracked: {OperationName} took {DurationMs}ms for user {UserId}",
                    operationName, durationMs, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track performance: {OperationName}", operationName);
            }
        }

        public async Task<List<ErrorLog>> GetRecentErrorsAsync(int count = 50)
        {
            try
            {
                return await _context.ErrorLogs
                    .OrderByDescending(e => e.OccurredAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent errors");
                return new List<ErrorLog>();
            }
        }

        public async Task<List<EventLog>> GetRecentEventsAsync(int count = 50)
        {
            try
            {
                return await _context.EventLogs
                    .OrderByDescending(e => e.OccurredAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent events");
                return new List<EventLog>();
            }
        }
    }
}