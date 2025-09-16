namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Admin 授權閘道介面
    /// 基於 database.json 定義的 ManagerData, ManagerRole, ManagerRolePermission 實作 RBAC
    /// </summary>
    public interface IMiniGameAdminGate
    {
        /// <summary>
        /// 檢查指定管理員是否有 MiniGame Admin 存取權限
        /// </summary>
        /// <param name="managerId">管理員 ID</param>
        /// <returns>是否有存取權限</returns>
        Task<bool> HasAccessAsync(int managerId);
    }
}