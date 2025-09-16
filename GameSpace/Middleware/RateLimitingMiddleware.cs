using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GameSpace.Middleware
{
    /// <summary>
    /// 速率限制中間件 - 防止暴力攻擊和 DDoS
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitOptions _options;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var endpoint = $"{context.Request.Method}:{context.Request.Path}";

            if (IsRateLimited(clientId, endpoint))
            {
                _logger.LogWarning("速率限制觸發: {ClientId} 嘗試訪問 {Endpoint}", clientId, endpoint);
                context.Response.StatusCode = 429;
                context.Response.Headers.Add("Retry-After", _options.WindowSeconds.ToString());
                await context.Response.WriteAsync("請求過於頻繁，請稍後再試");
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // 優先使用真實 IP，然後是連接 ID
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return $"{ip}:{userAgent.GetHashCode()}";
        }

        private bool IsRateLimited(string clientId, string endpoint)
        {
            var key = $"rate_limit:{clientId}:{endpoint}";
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(key, out RateLimitInfo info))
            {
                // 檢查是否在時間窗口內
                if (now - info.FirstRequest < TimeSpan.FromSeconds(_options.WindowSeconds))
                {
                    // 檢查請求次數
                    if (info.RequestCount >= _options.MaxRequests)
                    {
                        return true;
                    }
                    info.RequestCount++;
                }
                else
                {
                    // 重置計數器
                    info = new RateLimitInfo { FirstRequest = now, RequestCount = 1 };
                }
            }
            else
            {
                info = new RateLimitInfo { FirstRequest = now, RequestCount = 1 };
            }

            _cache.Set(key, info, TimeSpan.FromSeconds(_options.WindowSeconds));
            return false;
        }
    }

    public class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public int RequestCount { get; set; }
    }

    public class RateLimitOptions
    {
        public int MaxRequests { get; set; } = 100; // 每窗口最大請求數
        public int WindowSeconds { get; set; } = 60; // 時間窗口（秒）
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, RateLimitOptions? options = null)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>(options ?? new RateLimitOptions());
        }
    }
}