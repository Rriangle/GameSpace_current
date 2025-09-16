using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Middleware
{
    /// <summary>
    /// MiniGame Area 統一錯誤處理中間件
    /// 確保所有錯誤回應使用統一的ProblemDetails格式
    /// </summary>
    public class MiniGameErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiniGameErrorHandlingMiddleware> _logger;

        public MiniGameErrorHandlingMiddleware(RequestDelegate next, ILogger<MiniGameErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 只處理MiniGame Area的請求
                if (context.Request.Path.StartsWithSegments("/MiniGame"))
                {
                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    throw; // 讓其他中間件處理非MiniGame請求
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;
            var userId = GetCurrentUserId(context);

            // 記錄錯誤
            _logger.LogError(exception, "MiniGame Area 發生未處理異常: TraceID={TraceID}, UserID={UserID}, Path={Path}", 
                traceId, userId, context.Request.Path);

            var problemDetails = CreateProblemDetails(context, exception, traceId);

            context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(json);
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string traceId)
        {
            return exception switch
            {
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "存取被拒絕",
                    Detail = "您沒有權限執行此操作",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },

                ArgumentException argEx => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "參數錯誤",
                    Detail = argEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },

                InvalidOperationException invOpEx => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "操作衝突",
                    Detail = invOpEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },

                TimeoutException => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.RequestTimeout,
                    Title = "請求逾時",
                    Detail = "操作執行時間過長，請稍後再試",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                },

                _ => new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "系統錯誤",
                    Detail = "伺服器發生內部錯誤，請稍後再試",
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = traceId }
                }
            };
        }

        private static string? GetCurrentUserId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                return context.User.FindFirst("UserID")?.Value ?? 
                       context.User.FindFirst("sub")?.Value ?? 
                       context.User.FindFirst("id")?.Value;
            }
            return null;
        }
    }

    /// <summary>
    /// 中間件擴展方法
    /// </summary>
    public static class MiniGameErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseMiniGameErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiniGameErrorHandlingMiddleware>();
        }
    }
}