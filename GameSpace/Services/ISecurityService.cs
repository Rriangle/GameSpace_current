using System.Threading.Tasks;

namespace GameSpace.Services
{
    /// <summary>
    /// 安全性服務介面
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// 驗證密碼強度
        /// </summary>
        Task<bool> ValidatePasswordStrengthAsync(string password);

        /// <summary>
        /// 檢查帳號是否被鎖定
        /// </summary>
        Task<bool> IsAccountLockedAsync(string userAccount);

        /// <summary>
        /// 記錄登入失敗
        /// </summary>
        Task RecordFailedLoginAsync(string userAccount, string ipAddress);

        /// <summary>
        /// 重置登入失敗計數
        /// </summary>
        Task ResetFailedLoginCountAsync(string userAccount);

        /// <summary>
        /// 檢查 IP 是否在黑名單中
        /// </summary>
        Task<bool> IsIpBlacklistedAsync(string ipAddress);

        /// <summary>
        /// 記錄安全事件
        /// </summary>
        Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress);

        /// <summary>
        /// 驗證輸入內容是否安全
        /// </summary>
        Task<bool> ValidateInputSafetyAsync(string input);

        /// <summary>
        /// 清理 HTML 內容防止 XSS
        /// </summary>
        Task<string> SanitizeHtmlAsync(string html);
    }
}