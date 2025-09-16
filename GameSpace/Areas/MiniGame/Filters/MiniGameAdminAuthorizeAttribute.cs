using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.MiniGame.Filters
{
    /// <summary>
    /// MiniGame Admin 專用授權過濾器
    /// 檢查 ManagerRolePermission.Pet_Rights_Management 權限
    /// </summary>
    public sealed class MiniGameAdminAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 在 Action 執行前檢查 MiniGame Admin 權限
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IMiniGameAdminAuthService>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<MiniGameAdminAuthorizeAttribute>>();
            
            // 取得當前管理員 ID
            var managerId = authService.GetCurrentManagerId(context.HttpContext.User);
            
            if (managerId == null)
            {
                logger.LogWarning("MiniGame Admin 存取被拒：無法識別管理員身份, TraceId={TraceId}", 
                    context.HttpContext.TraceIdentifier);
                
                context.Result = new ForbidResult();
                return;
            }

            // 檢查 Pet_Rights_Management 權限
            var hasPermission = await authService.CanAccessAsync(managerId.Value);
            
            if (!hasPermission)
            {
                logger.LogWarning("MiniGame Admin 存取被拒：權限不足, ManagerId={ManagerId}, TraceId={TraceId}", 
                    managerId.Value, context.HttpContext.TraceIdentifier);
                
                // 返回 403 Forbidden 或重導向到無權限頁面
                context.Result = new ViewResult
                {
                    ViewName = "~/Areas/MiniGame/Views/Shared/NoPermission.cshtml",
                    StatusCode = 403
                };
                return;
            }

            // 權限檢查通過，繼續執行
            logger.LogInformation("MiniGame Admin 存取允許: ManagerId={ManagerId}, Action={Action}, TraceId={TraceId}", 
                managerId.Value, context.ActionDescriptor.DisplayName, context.HttpContext.TraceIdentifier);
            
            await next();
        }
    }
}