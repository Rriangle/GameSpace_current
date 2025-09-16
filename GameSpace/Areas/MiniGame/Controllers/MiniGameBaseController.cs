using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area 控制器基類
    /// 提供統一的錯誤處理和回應格式
    /// </summary>
    [Area("MiniGame")]
    public abstract class MiniGameBaseController : Controller
    {
        /// <summary>
        /// 成功回應 (200 OK)
        /// </summary>
        protected IActionResult Success(object? data = null, string message = "操作成功")
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data,
                traceId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// 成功回應 (201 Created)
        /// </summary>
        protected IActionResult Created(object? data = null, string message = "創建成功")
        {
            return StatusCode((int)HttpStatusCode.Created, new
            {
                success = true,
                message = message,
                data = data,
                traceId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// 錯誤回應 - 使用ProblemDetails格式
        /// </summary>
        protected IActionResult Error(string title, string detail, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = HttpContext.Request.Path,
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            };

            if (extensions != null)
            {
                foreach (var prop in extensions.GetType().GetProperties())
                {
                    problemDetails.Extensions[prop.Name] = prop.GetValue(extensions);
                }
            }

            return StatusCode((int)statusCode, problemDetails);
        }

        /// <summary>
        /// 參數錯誤回應 (400 Bad Request)
        /// </summary>
        protected IActionResult BadRequest(string detail, object? extensions = null)
        {
            return Error("參數錯誤", detail, HttpStatusCode.BadRequest, extensions);
        }

        /// <summary>
        /// 未授權回應 (401 Unauthorized)
        /// </summary>
        protected IActionResult Unauthorized(string detail = "您沒有權限執行此操作")
        {
            return Error("存取被拒絕", detail, HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// 找不到資源回應 (404 Not Found)
        /// </summary>
        protected IActionResult NotFound(string detail, object? extensions = null)
        {
            return Error("資源不存在", detail, HttpStatusCode.NotFound, extensions);
        }

        /// <summary>
        /// 衝突回應 (409 Conflict)
        /// </summary>
        protected IActionResult Conflict(string detail, object? extensions = null)
        {
            return Error("操作衝突", detail, HttpStatusCode.Conflict, extensions);
        }

        /// <summary>
        /// 伺服器錯誤回應 (500 Internal Server Error)
        /// </summary>
        protected IActionResult InternalServerError(string detail = "伺服器發生內部錯誤，請稍後再試", object? extensions = null)
        {
            return Error("系統錯誤", detail, HttpStatusCode.InternalServerError, extensions);
        }

        /// <summary>
        /// 取得當前登入會員 ID
        /// </summary>
        protected int GetCurrentUserID()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst("sub") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userID))
                {
                    return userID;
                }
            }
            throw new UnauthorizedAccessException("無法取得會員身份資訊");
        }

        /// <summary>
        /// 安全的取得當前會員ID (不拋出異常)
        /// </summary>
        protected int? TryGetCurrentUserID()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst("sub") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userID))
                {
                    return userID;
                }
            }
            return null;
        }

        /// <summary>
        /// 檢查模型驗證狀態並回傳錯誤
        /// </summary>
        protected IActionResult? ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );

                return BadRequest("輸入資料驗證失敗", new { errors });
            }
            return null;
        }
    }
}