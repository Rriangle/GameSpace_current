using GameSpace.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Admin 授權閘道實作
    /// 嚴格按照 database.json 定義實作 RBAC 檢查
    /// </summary>
    public class MiniGameAdminGate : IMiniGameAdminGate
    {
        private readonly GameSpaceDbContext _context;
        private readonly ILogger<MiniGameAdminGate> _logger;

        public MiniGameAdminGate(GameSpaceDbContext context, ILogger<MiniGameAdminGate> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasAccessAsync(int managerId)
        {
            try
            {
                // 1. 檢查帳號狀態 (ManagerData)
                // 使用原始 SQL 查詢以確保與 database.json 完全匹配
                var accountStateQuery = @"
                    SELECT Manager_Id, Manager_EmailConfirmed, Manager_LockoutEnabled, Manager_LockoutEnd
                    FROM dbo.ManagerData 
                    WHERE Manager_Id = {0}";

                var accountData = await _context.Database
                    .SqlQueryRaw<AccountStateResult>(accountStateQuery, managerId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                bool emailConfirmed = accountData?.Manager_EmailConfirmed ?? false;
                bool lockoutEnabled = accountData?.Manager_LockoutEnabled ?? true;
                DateTime? lockoutEnd = accountData?.Manager_LockoutEnd;

                // 檢查帳號狀態：必須 EmailConfirmed=1 且 (LockoutEnabled=0 或 LockoutEnd 已過期)
                bool okAccount = emailConfirmed && 
                    (!lockoutEnabled || lockoutEnd == null || lockoutEnd < DateTime.UtcNow);

                // 2. 檢查權限 (ManagerRole → ManagerRolePermission)
                var permissionQuery = @"
                    SELECT r.ManagerRole_Id, p.Pet_Rights_Management
                    FROM dbo.ManagerRole r
                    JOIN dbo.ManagerRolePermission p ON p.ManagerRole_Id = r.ManagerRole_Id
                    WHERE r.Manager_Id = {0}";

                var permissions = await _context.Database
                    .SqlQueryRaw<PermissionResult>(permissionQuery, managerId)
                    .AsNoTracking()
                    .ToListAsync();

                int roleCount = permissions.Count;
                bool hasPetRight = permissions.Any(p => p.Pet_Rights_Management);

                // 3. 最終決定：帳號狀態 OK 且 有任一角色具備 Pet_Rights_Management=1
                bool result = okAccount && hasPetRight;

                // 4. 審計日誌 (單行格式)
                string lockoutStatus = lockoutEnabled ? 
                    (lockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss") ?? "enabled") : "disabled";

                _logger.LogInformation(
                    "RBAC MiniGame: manager={ManagerId} emailConfirmed={EmailConfirmed} lockout={LockoutStatus} roles={RoleCount} petRight={PetRight} result={Result}",
                    managerId, emailConfirmed ? 1 : 0, lockoutStatus, roleCount, hasPetRight ? 1 : 0, result ? "ALLOW" : "DENY");

                return result;
            }
            catch (Exception ex)
            {
                // 任何異常都拒絕存取 (fail-safe)
                _logger.LogError(ex, "RBAC MiniGame: manager={ManagerId} result=DENY (exception)", managerId);
                return false;
            }
        }

        /// <summary>
        /// 帳號狀態查詢結果 (對應 database.json ManagerData 欄位)
        /// </summary>
        public class AccountStateResult
        {
            public int Manager_Id { get; set; }
            public bool Manager_EmailConfirmed { get; set; }
            public bool Manager_LockoutEnabled { get; set; }
            public DateTime? Manager_LockoutEnd { get; set; }
        }

        /// <summary>
        /// 權限查詢結果 (對應 database.json ManagerRole + ManagerRolePermission 欄位)
        /// </summary>
        public class PermissionResult
        {
            public int ManagerRole_Id { get; set; }
            public bool Pet_Rights_Management { get; set; }
        }
    }
}