using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameSpace.Middleware
{
    /// <summary>
    /// CSRF 保護中間件 - 防止跨站請求偽造攻擊
    /// </summary>
    public class CsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDataProtector _protector;
        private readonly ILogger<CsrfProtectionMiddleware> _logger;
        private const string CsrfTokenName = "__RequestVerificationToken";

        public CsrfProtectionMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider, ILogger<CsrfProtectionMiddleware> logger)
        {
            _next = next;
            _protector = dataProtectionProvider.CreateProtector("CSRF");
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 為 GET 請求生成 CSRF Token
            if (context.Request.Method == "GET")
            {
                GenerateCsrfToken(context);
            }
            // 為 POST/PUT/DELETE 請求驗證 CSRF Token
            else if (IsStateChangingMethod(context.Request.Method))
            {
                if (!ValidateCsrfToken(context))
                {
                    _logger.LogWarning("CSRF Token 驗證失敗: {RemoteIP}", context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("CSRF Token 驗證失敗");
                    return;
                }
            }

            await _next(context);
        }

        private void GenerateCsrfToken(HttpContext context)
        {
            var token = GenerateRandomToken();
            var protectedToken = _protector.Protect(token);
            
            // 將 Token 存儲在 Session 中
            context.Session.SetString(CsrfTokenName, protectedToken);
            
            // 將 Token 添加到 ViewData 中供視圖使用
            context.Items[CsrfTokenName] = protectedToken;
        }

        private bool ValidateCsrfToken(HttpContext context)
        {
            // 從表單或 Header 中獲取 Token
            var submittedToken = context.Request.Form[CsrfTokenName].ToString() 
                               ?? context.Request.Headers["X-CSRF-Token"].ToString();

            if (string.IsNullOrEmpty(submittedToken))
            {
                return false;
            }

            // 從 Session 中獲取原始 Token
            var sessionToken = context.Session.GetString(CsrfTokenName);
            if (string.IsNullOrEmpty(sessionToken))
            {
                return false;
            }

            try
            {
                var unprotectedSessionToken = _protector.Unprotect(sessionToken);
                var unprotectedSubmittedToken = _protector.Unprotect(submittedToken);
                
                return unprotectedSessionToken == unprotectedSubmittedToken;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static bool IsStateChangingMethod(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class CsrfProtectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsrfProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfProtectionMiddleware>();
        }
    }
}