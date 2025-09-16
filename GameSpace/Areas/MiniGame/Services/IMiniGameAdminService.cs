using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Area Admin 服務介面
    /// 提供 MiniGame Area 後台管理的業務邏輯抽象
    /// 採介面＋ DI 設計，查詢以 AsNoTracking() 投影至 ReadModel
    /// </summary>
    public interface IMiniGameAdminService
    {
        #region User_Wallet 模組服務

        /// <summary>
        /// 取得錢包統計摘要
        /// </summary>
        Task<WalletSummaryReadModel> GetWalletSummaryAsync();

        /// <summary>
        /// 取得錢包分頁列表
        /// </summary>
        Task<PagedResult<WalletReadModel>> GetWalletsAsync(WalletQueryModel query);

        /// <summary>
        /// 取得錢包明細
        /// </summary>
        Task<WalletDetailReadModel?> GetWalletDetailAsync(int userId);

        #endregion

        #region UserSignInStats 模組服務

        /// <summary>
        /// 取得簽到統計摘要
        /// </summary>
        Task<SignInSummaryReadModel> GetSignInSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得簽到記錄分頁列表
        /// </summary>
        Task<PagedResult<SignInStatsReadModel>> GetSignInStatsAsync(SignInQueryModel query);

        /// <summary>
        /// 取得用戶簽到歷史
        /// </summary>
        Task<UserSignInHistoryReadModel> GetUserSignInHistoryAsync(int userId);

        #endregion

        #region Pet 模組服務

        /// <summary>
        /// 取得寵物統計摘要
        /// </summary>
        Task<PetSummaryReadModel> GetPetSummaryAsync();

        /// <summary>
        /// 取得寵物分頁列表
        /// </summary>
        Task<PagedResult<PetReadModel>> GetPetsAsync(PetQueryModel query);

        /// <summary>
        /// 取得寵物明細
        /// </summary>
        Task<PetDetailReadModel?> GetPetDetailAsync(int petId);

        #endregion

        #region MiniGame 模組服務

        /// <summary>
        /// 取得遊戲統計摘要
        /// </summary>
        Task<GameSummaryReadModel> GetGameSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得遊戲記錄分頁列表
        /// </summary>
        Task<PagedResult<GameRecordReadModel>> GetGameRecordsAsync(GameQueryModel query);

        /// <summary>
        /// 取得遊戲明細
        /// </summary>
        Task<GameDetailReadModel?> GetGameDetailAsync(int playId);

        #endregion
    }

    #region ReadModel 定義

    /// <summary>
    /// 分頁結果包裝器
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 錢包查詢模型
    /// </summary>
    public class WalletQueryModel
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 錢包讀取模型
    /// </summary>
    public class WalletReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserNickName { get; set; } = string.Empty;
        public int UserPoint { get; set; }
        public string PointsLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// 錢包統計摘要
    /// </summary>
    public class WalletSummaryReadModel
    {
        public int TotalWallets { get; set; }
        public long TotalPoints { get; set; }
        public decimal AveragePoints { get; set; }
        public int VipMembers { get; set; }
    }

    /// <summary>
    /// 錢包明細讀取模型
    /// </summary>
    public class WalletDetailReadModel
    {
        public UserWallet Wallet { get; set; } = new();
        public List<WalletHistory> RecentHistory { get; set; } = new();
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
    }

    /// <summary>
    /// 簽到查詢模型
    /// </summary>
    public class SignInQueryModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }
        public string? UserSearch { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 簽到統計讀取模型
    /// </summary>
    public class SignInStatsReadModel
    {
        public int LogID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserNickName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsChanged { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到統計摘要
    /// </summary>
    public class SignInSummaryReadModel
    {
        public int TotalSignIns { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public int TotalCoupons { get; set; }
        public int UniqueUsers { get; set; }
        public List<DailySignInStats> DailyStats { get; set; } = new();
    }

    /// <summary>
    /// 每日簽到統計
    /// </summary>
    public class DailySignInStats
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public int CouponsGiven { get; set; }
    }

    /// <summary>
    /// 用戶簽到歷史
    /// </summary>
    public class UserSignInHistoryReadModel
    {
        public User User { get; set; } = new();
        public List<UserSignInStats> SignInHistory { get; set; } = new();
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalSignInDays { get; set; }
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel
    {
        public string? Search { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物讀取模型
    /// </summary>
    public class PetReadModel
    {
        public int PetID { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public DateTime LevelUpTime { get; set; }
    }

    /// <summary>
    /// 寵物統計摘要
    /// </summary>
    public class PetSummaryReadModel
    {
        public int TotalPets { get; set; }
        public double AverageLevel { get; set; }
        public int MaxLevel { get; set; }
        public long TotalExperience { get; set; }
        public double AverageHealth { get; set; }
    }

    /// <summary>
    /// 寵物明細讀取模型
    /// </summary>
    public class PetDetailReadModel
    {
        public Pet Pet { get; set; } = new();
        public List<Models.MiniGame> RecentGames { get; set; } = new();
        public int TotalGamesPlayed { get; set; }
        public double WinRate { get; set; }
    }

    /// <summary>
    /// 遊戲查詢模型
    /// </summary>
    public class GameQueryModel
    {
        public string? Result { get; set; }
        public int? Level { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 遊戲記錄讀取模型
    /// </summary>
    public class GameRecordReadModel
    {
        public int PlayID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PointsChanged { get; set; }
        public int ExpGained { get; set; }
        public bool Aborted { get; set; }
    }

    /// <summary>
    /// 遊戲統計摘要
    /// </summary>
    public class GameSummaryReadModel
    {
        public int TotalGames { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int AbortCount { get; set; }
        public double WinRate { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public List<LevelStats> LevelStats { get; set; } = new();
        public List<DailyGameStats> DailyStats { get; set; } = new();
    }

    /// <summary>
    /// 關卡統計
    /// </summary>
    public class LevelStats
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public double WinRate { get; set; }
        public double AvgPoints { get; set; }
        public double AvgExp { get; set; }
    }

    /// <summary>
    /// 每日遊戲統計
    /// </summary>
    public class DailyGameStats
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int AbortCount { get; set; }
    }

    /// <summary>
    /// 遊戲明細讀取模型
    /// </summary>
    public class GameDetailReadModel
    {
        public Models.MiniGame Game { get; set; } = new();
        public Pet Pet { get; set; } = new();
        public User User { get; set; } = new();
        public TimeSpan? Duration { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
    }

    #endregion
}