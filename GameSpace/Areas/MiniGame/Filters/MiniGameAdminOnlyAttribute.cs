using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.MiniGame.Filters
{
    /// <summary>
    /// MiniGame Admin 專用授權過濾器
    /// 基於 database.json 定義的 RBAC 規則進行授權檢查
    /// </summary>
    public class MiniGameAdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<MiniGameAdminOnlyAttribute>>();
            var adminGate = context.HttpContext.RequestServices.GetRequiredService<IMiniGameAdminGate>();

            try
            {
                // 1. 取得當前管理員 ID
                var managerId = GetCurrentManagerId(context.HttpContext);
                if (managerId == null)
                {
                    logger.LogWarning("RBAC MiniGame: 無法解析 Manager_Id，請確認驗證中介軟體設定正確的 Claims");
                    context.Result = new ForbidResult();
                    return;
                }

                // 2. 檢查授權
                bool hasAccess = await adminGate.HasAccessAsync(managerId.Value);
                if (!hasAccess)
                {
                    context.Result = new ForbidResult(); // 403 Forbidden
                    return;
                }

                // 授權通過，繼續處理請求
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RBAC MiniGame: 授權檢查發生異常，拒絕存取");
                context.Result = new ForbidResult();
            }
        }

        /// <summary>
        /// 從 HttpContext 取得當前管理員 ID
        /// 支援多種 Claim 類型以提高相容性
        /// </summary>
        private static int? GetCurrentManagerId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // 嘗試從不同的 Claim 類型取得 Manager ID
            var claimTypes = new[] { "ManagerId", "Manager_Id", "UserId", "UserID", "sub", "id" };
            
            foreach (var claimType in claimTypes)
            {
                var claim = context.User.FindFirst(claimType);
                if (claim != null && int.TryParse(claim.Value, out var managerId))
                {
                    return managerId;
                }
            }

            return null;
        }
    }
}