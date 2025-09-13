using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GameSpace.Middleware
{
    /// <summary>
    /// 安全性標頭中間件 - 設定各種安全性 HTTP 標頭
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 設定安全性標頭
            SetSecurityHeaders(context);

            await _next(context);
        }

        private void SetSecurityHeaders(HttpContext context)
        {
            var response = context.Response;

            // 防止點擊劫持攻擊
            response.Headers.Add("X-Frame-Options", "DENY");

            // 防止 MIME 類型嗅探
            response.Headers.Add("X-Content-Type-Options", "nosniff");

            // 啟用 XSS 防護
            response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // 強制 HTTPS
            response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

            // 內容安全政策 (CSP)
            response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' https:; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'");

            // 引用者政策
            response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // 權限政策
            response.Headers.Add("Permissions-Policy", 
                "geolocation=(), " +
                "microphone=(), " +
                "camera=(), " +
                "payment=(), " +
                "usb=(), " +
                "magnetometer=(), " +
                "gyroscope=(), " +
                "speaker=(), " +
                "vibrate=(), " +
                "fullscreen=(self), " +
                "sync-xhr=()");

            // 移除伺服器資訊
            response.Headers.Remove("Server");
            response.Headers.Remove("X-Powered-By");

            _logger.LogDebug("安全性標頭已設定");
        }
    }

    /// <summary>
    /// 安全性標頭中間件擴展方法
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}