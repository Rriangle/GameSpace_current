using GameSpace.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Admin 認證服務實作
    /// 基於 ManagerRolePermission.Pet_Rights_Management 進行權限檢查
    /// </summary>
    public class MiniGameAdminAuthService : IMiniGameAdminAuthService
    {
        private readonly GameSpaceDbContext _context;
        private readonly ILogger<MiniGameAdminAuthService> _logger;

        public MiniGameAdminAuthService(GameSpaceDbContext context, ILogger<MiniGameAdminAuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 檢查管理員是否可以存取 MiniGame Admin 功能
        /// 查詢 ManagerRole ↔ ManagerRolePermission 連結
        /// </summary>
        /// <param name="managerId">管理員 ID</param>
        /// <returns>true 表示具備 Pet_Rights_Management 權限</returns>
        public async Task<bool> CanAccessAsync(int managerId)
        {
            try
            {
                var hasPermission = await (from mr in _context.ManagerRoles.AsNoTracking()
                                         join mrp in _context.ManagerRolePermissions.AsNoTracking()
                                           on mr.ManagerRole_Id equals mrp.ManagerRole_Id
                                         where mr.Manager_Id == managerId
                                         select mrp.Pet_Rights_Management)
                                         .AnyAsync(x => x == true);

                _logger.LogInformation("MiniGame Admin 權限檢查: ManagerId={ManagerId}, HasPermission={HasPermission}", 
                    managerId, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MiniGame Admin 權限檢查失敗: ManagerId={ManagerId}", managerId);
                return false; // 預設拒絕存取
            }
        }

        /// <summary>
        /// 從 HttpContext 取得當前管理員 ID
        /// </summary>
        /// <param name="user">當前用戶 Principal</param>
        /// <returns>管理員 ID，若無效則返回 null</returns>
        public int? GetCurrentManagerId(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // 嘗試多種 Claims 類型
            var managerIdClaim = user.FindFirst("ManagerId") ?? 
                                user.FindFirst("Manager_Id") ?? 
                                user.FindFirst("sub") ?? 
                                user.FindFirst("id") ??
                                user.FindFirst(ClaimTypes.NameIdentifier);

            if (managerIdClaim != null && int.TryParse(managerIdClaim.Value, out var managerId))
            {
                return managerId;
            }

            return null;
        }
    }
}