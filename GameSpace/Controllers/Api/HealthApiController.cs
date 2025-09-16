using GameSpace.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthApiController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly IErrorTrackingService _errorTrackingService;
        private readonly ILogger<HealthApiController> _logger;

        public HealthApiController(
            IHealthCheckService healthCheckService,
            IErrorTrackingService errorTrackingService,
            ILogger<HealthApiController> logger)
        {
            _healthCheckService = healthCheckService;
            _errorTrackingService = errorTrackingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                var healthChecks = await _healthCheckService.PerformHealthChecksAsync();
                var isHealthy = healthChecks.Values.All(status => status == "Healthy" || status == "Not Configured");

                var response = new
                {
                    Status = isHealthy ? "Healthy" : "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Checks = healthChecks
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _errorTrackingService.TrackErrorAsync(ex, GetCurrentUserId());
                _logger.LogError(ex, "Health check failed");
                
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Health check failed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealthStatus()
        {
            try
            {
                var healthChecks = await _healthCheckService.PerformHealthChecksAsync();
                var recentErrors = await _errorTrackingService.GetRecentErrorsAsync(10);
                var recentEvents = await _errorTrackingService.GetRecentEventsAsync(10);

                var response = new
                {
                    Status = healthChecks.Values.All(status => status == "Healthy" || status == "Not Configured") ? "Healthy" : "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Checks = healthChecks,
                    RecentErrors = recentErrors.Select(e => new
                    {
                        e.ExceptionType,
                        e.Message,
                        e.OccurredAt,
                        e.UserId,
                        e.RequestPath
                    }),
                    RecentEvents = recentEvents.Select(e => new
                    {
                        e.EventName,
                        e.OccurredAt,
                        e.UserId,
                        e.RequestPath
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _errorTrackingService.TrackErrorAsync(ex, GetCurrentUserId());
                _logger.LogError(ex, "Detailed health check failed");
                
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Detailed health check failed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetRecentErrors([FromQuery] int count = 50)
        {
            try
            {
                var errors = await _errorTrackingService.GetRecentErrorsAsync(count);
                return Ok(errors);
            }
            catch (Exception ex)
            {
                await _errorTrackingService.TrackErrorAsync(ex, GetCurrentUserId());
                _logger.LogError(ex, "Failed to get recent errors");
                return StatusCode(500, "Failed to get recent errors");
            }
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetRecentEvents([FromQuery] int count = 50)
        {
            try
            {
                var events = await _errorTrackingService.GetRecentEventsAsync(count);
                return Ok(events);
            }
            catch (Exception ex)
            {
                await _errorTrackingService.TrackErrorAsync(ex, GetCurrentUserId());
                _logger.LogError(ex, "Failed to get recent events");
                return StatusCode(500, "Failed to get recent events");
            }
        }

        private string? GetCurrentUserId()
        {
            return User?.Identity?.Name ?? User?.FindFirst("sub")?.Value;
        }
    }
}