using GameSpace.Services;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameSpace.Middleware
{
    /// <summary>
    /// 效能監控中介軟體
    /// </summary>
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPerformanceService _performanceService;
        private readonly ILogger<PerformanceMiddleware> _logger;

        public PerformanceMiddleware(RequestDelegate next, IPerformanceService performanceService, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _performanceService = performanceService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // 記錄請求開始
                _logger.LogDebug("開始處理請求: {Method} {Path}", context.Request.Method, context.Request.Path);

                // 執行下一個中介軟體
                await _next(context);

                // 記錄響應時間
                stopwatch.Stop();
                await _performanceService.LogApiResponseTimeAsync(
                    context.Request.Path,
                    context.Request.Method,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);

                _logger.LogDebug("請求處理完成: {Method} {Path} {StatusCode} {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // 記錄錯誤
                _logger.LogError(ex, "請求處理失敗: {Method} {Path} {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);

                // 記錄錯誤響應時間
                await _performanceService.LogApiResponseTimeAsync(
                    context.Request.Path,
                    context.Request.Method,
                    stopwatch.ElapsedMilliseconds,
                    500);

                throw;
            }
        }
    }

    /// <summary>
    /// 效能監控中介軟體擴展方法
    /// </summary>
    public static class PerformanceMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMiddleware>();
        }
    }
}