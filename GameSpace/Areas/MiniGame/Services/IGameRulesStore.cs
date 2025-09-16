using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 遊戲規則配置選項
    /// </summary>
    public class GameRulesOptions
    {
        public MetadataOptions Metadata { get; set; } = new();
        public GameRulesSettings GameRules { get; set; } = new();
        public RewardTablesOptions RewardTables { get; set; } = new();
        public Dictionary<string, MonsterWaveOptions> MonsterWaves { get; set; } = new();
        public PetRulesOptions PetRules { get; set; } = new();
        public SignInRulesOptions SignInRules { get; set; } = new();
        public ValidationOptions Validation { get; set; } = new();
    }

    public class MetadataOptions
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = "";
        public string Timezone { get; set; } = "Asia/Taipei";
    }

    public class GameRulesSettings
    {
        [Range(1, 10)]
        public int DailyLimit { get; set; } = 3;
        
        public string ResetTime { get; set; } = "00:00";
        public string Timezone { get; set; } = "Asia/Taipei";
        
        [Range(5, 120)]
        public int SessionTimeoutMinutes { get; set; } = 30;
    }

    public class RewardTablesOptions
    {
        public Dictionary<string, int> BasePointsPerLevel { get; set; } = new();
        public Dictionary<string, int> ExpPerLevel { get; set; } = new();
        public MultiplierOptions Multipliers { get; set; } = new();
    }

    public class MultiplierOptions
    {
        public double Win { get; set; } = 1.5;
        public double Lose { get; set; } = 0.5;
        public double Abort { get; set; } = 0.0;
    }

    public class MonsterWaveOptions
    {
        [Range(1, 50)]
        public int MonsterCount { get; set; }
        
        [Range(0.1, 5.0)]
        public double Speed { get; set; }
        
        public string Difficulty { get; set; } = "";
    }

    public class PetRulesOptions
    {
        public int RenameCost { get; set; } = 0;
        public int SkinColorChangeCost { get; set; } = 50;
        public int BackgroundColorChangeCost { get; set; } = 0;
        public Dictionary<string, int> LevelUpExpRequirement { get; set; } = new();
        public AvailableColorsOptions AvailableColors { get; set; } = new();
    }

    public class AvailableColorsOptions
    {
        public List<string> SkinColors { get; set; } = new();
        public List<string> BackgroundColors { get; set; } = new();
    }

    public class SignInRulesOptions
    {
        public int BaseRewardPoints { get; set; } = 10;
        public double ConsecutiveBonusMultiplier { get; set; } = 1.1;
        public int MaxConsecutiveDays { get; set; } = 7;
        public string ResetTime { get; set; } = "00:00";
        public string Timezone { get; set; } = "Asia/Taipei";
    }

    public class ValidationOptions
    {
        public RangeOptions DailyLimit { get; set; } = new();
        public RangeOptions BasePointsPerLevel { get; set; } = new();
        public RangeOptions MonsterCount { get; set; } = new();
        public RangeDoubleOptions Speed { get; set; } = new();
    }

    public class RangeOptions
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class RangeDoubleOptions
    {
        public double Min { get; set; }
        public double Max { get; set; }
    }

    /// <summary>
    /// 遊戲規則存儲介面
    /// </summary>
    public interface IGameRulesStore
    {
        /// <summary>
        /// 讀取遊戲規則配置
        /// </summary>
        Task<GameRulesOptions> GetRulesAsync();

        /// <summary>
        /// 保存遊戲規則配置
        /// </summary>
        Task SaveRulesAsync(GameRulesOptions rules);

        /// <summary>
        /// 驗證規則配置有效性
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateRulesAsync(GameRulesOptions rules);

        /// <summary>
        /// 重新載入配置（清除快取）
        /// </summary>
        Task ReloadAsync();
    }
}