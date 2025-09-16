using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace GameSpace.Areas.MiniGame.Filters
{
    /// <summary>
    /// MiniGame Area 專用 ProblemDetails 異常篩檢器
    /// 捕獲未處理異常並回傳 RFC 7807 格式的錯誤回應（zh-TW）
    /// </summary>
    public class MiniGameProblemDetailsFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _environment;

        public MiniGameProblemDetailsFilter(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            var exception = context.Exception;
            var request = context.HttpContext.Request;
            
            // 根據異常類型決定狀態碼
            var statusCode = GetStatusCodeFromException(exception);
            
            // 建立 ProblemDetails 回應
            var problemDetails = new ProblemDetails
            {
                Title = "系統錯誤",
                Detail = GetSafeErrorMessage(exception),
                Status = statusCode,
                Instance = request.Path.Value,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            // 新增擴展資訊
            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            problemDetails.Extensions["area"] = "MiniGame";
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            
            // 開發環境下包含更多除錯資訊
            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            // 設定回應
            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
            
            context.HttpContext.Response.ContentType = "application/problem+json";
            context.ExceptionHandled = true;
        }

        private static int GetStatusCodeFromException(Exception exception)
        {
            return exception switch
            {
                TaskCanceledException or OperationCanceledException => (int)HttpStatusCode.RequestTimeout, // 408
                TimeoutException => (int)HttpStatusCode.RequestTimeout, // 408
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                ArgumentException or ArgumentNullException => (int)HttpStatusCode.BadRequest, // 400
                KeyNotFoundException or FileNotFoundException => (int)HttpStatusCode.NotFound, // 404
                _ => (int)HttpStatusCode.InternalServerError // 500
            };
        }

        private string GetSafeErrorMessage(Exception exception)
        {
            // 在生產環境中隱藏敏感的異常細節
            if (_environment.IsProduction())
            {
                return exception switch
                {
                    TaskCanceledException or OperationCanceledException => "請求處理超時，請稍後重試",
                    TimeoutException => "資料庫查詢超時，請縮小查詢範圍或稍後重試",
                    UnauthorizedAccessException => "存取權限不足",
                    ArgumentException => "請求參數無效",
                    KeyNotFoundException => "找不到指定的資源",
                    _ => "系統發生未預期的錯誤，請聯繫管理員"
                };
            }

            // 開發環境回傳完整錯誤訊息
            return exception.Message;
        }
    }
}