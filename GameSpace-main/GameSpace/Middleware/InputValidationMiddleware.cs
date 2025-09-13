using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameSpace.Middleware
{
    /// <summary>
    /// 輸入驗證中間件 - 防止 XSS、SQL 注入等攻擊
    /// </summary>
    public class InputValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputValidationMiddleware> _logger;

        // 危險字元模式
        private static readonly Regex[] DangerousPatterns = {
            new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"union\s+select", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"drop\s+table", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"insert\s+into", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"delete\s+from", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"update\s+set", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"exec\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"sp_", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 檢查查詢參數
            if (context.Request.QueryString.HasValue)
            {
                var queryString = context.Request.QueryString.Value;
                if (ContainsDangerousContent(queryString))
                {
                    _logger.LogWarning("檢測到危險查詢參數: {QueryString} from {RemoteIP}", 
                        queryString, context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("無效的請求參數");
                    return;
                }
            }

            // 檢查 POST 表單數據
            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                foreach (var field in form)
                {
                    foreach (var value in field.Value)
                    {
                        if (ContainsDangerousContent(value))
                        {
                            _logger.LogWarning("檢測到危險表單數據: {Field}={Value} from {RemoteIP}", 
                                field.Key, value, context.Connection.RemoteIpAddress);
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("無效的表單數據");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }

        private static bool ContainsDangerousContent(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            foreach (var pattern in DangerousPatterns)
            {
                if (pattern.IsMatch(input))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class InputValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseInputValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InputValidationMiddleware>();
        }
    }
}