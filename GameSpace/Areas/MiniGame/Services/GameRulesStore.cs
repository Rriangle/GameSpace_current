using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 遊戲規則存儲實作 - 基於 JSON 文件
    /// </summary>
    public class GameRulesStore : IGameRulesStore
    {
        private readonly string _configPath;
        private readonly ILogger<GameRulesStore> _logger;
        private readonly SemaphoreSlim _fileLock;
        private GameRulesOptions? _cachedRules;
        private DateTime _lastReadTime;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public GameRulesStore(IWebHostEnvironment environment, ILogger<GameRulesStore> logger)
        {
            _configPath = Path.Combine(environment.ContentRootPath, "Areas", "MiniGame", "config", "game_rules.json");
            _logger = logger;
            _fileLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// 讀取遊戲規則配置
        /// </summary>
        public async Task<GameRulesOptions> GetRulesAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                // 檢查快取是否需要更新
                var fileInfo = new FileInfo(_configPath);
                if (_cachedRules != null && fileInfo.Exists && fileInfo.LastWriteTime <= _lastReadTime)
                {
                    return _cachedRules;
                }

                if (!File.Exists(_configPath))
                {
                    _logger.LogWarning("遊戲規則配置文件不存在: {ConfigPath}，使用預設值", _configPath);
                    return GetDefaultRules();
                }

                var jsonContent = await File.ReadAllTextAsync(_configPath);
                var rules = JsonSerializer.Deserialize<GameRulesOptions>(jsonContent, _jsonOptions);
                
                if (rules == null)
                {
                    _logger.LogError("無法解析遊戲規則配置文件: {ConfigPath}", _configPath);
                    return GetDefaultRules();
                }

                _cachedRules = rules;
                _lastReadTime = DateTime.UtcNow;

                _logger.LogInformation("成功讀取遊戲規則配置: DailyLimit={DailyLimit}, Version={Version}", 
                    rules.GameRules.DailyLimit, rules.Metadata.Version);

                return rules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讀取遊戲規則配置失敗: {ConfigPath}", _configPath);
                return GetDefaultRules();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 保存遊戲規則配置
        /// </summary>
        public async Task SaveRulesAsync(GameRulesOptions rules)
        {
            var (isValid, errors) = await ValidateRulesAsync(rules);
            if (!isValid)
            {
                throw new ValidationException($"遊戲規則驗證失敗: {string.Join(", ", errors)}");
            }

            await _fileLock.WaitAsync();
            try
            {
                // 更新元數據
                rules.Metadata.LastUpdated = DateTime.UtcNow;
                rules.Metadata.Version = IncrementVersion(rules.Metadata.Version);

                // 創建目錄（如果不存在）
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 寫入文件
                var jsonContent = JsonSerializer.Serialize(rules, _jsonOptions);
                await File.WriteAllTextAsync(_configPath, jsonContent);

                // 更新快取
                _cachedRules = rules;
                _lastReadTime = DateTime.UtcNow;

                _logger.LogInformation("成功保存遊戲規則配置: DailyLimit={DailyLimit}, Version={Version}", 
                    rules.GameRules.DailyLimit, rules.Metadata.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存遊戲規則配置失敗: {ConfigPath}", _configPath);
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 驗證規則配置有效性
        /// </summary>
        public async Task<(bool IsValid, List<string> Errors)> ValidateRulesAsync(GameRulesOptions rules)
        {
            var errors = new List<string>();

            // 驗證每日限制
            if (rules.GameRules.DailyLimit < rules.Validation.DailyLimit.Min || 
                rules.GameRules.DailyLimit > rules.Validation.DailyLimit.Max)
            {
                errors.Add($"每日遊戲限制必須在 {rules.Validation.DailyLimit.Min}-{rules.Validation.DailyLimit.Max} 之間");
            }

            // 驗證怪物波次配置
            foreach (var (level, wave) in rules.MonsterWaves)
            {
                if (wave.MonsterCount < rules.Validation.MonsterCount.Min || 
                    wave.MonsterCount > rules.Validation.MonsterCount.Max)
                {
                    errors.Add($"等級 {level} 怪物數量必須在 {rules.Validation.MonsterCount.Min}-{rules.Validation.MonsterCount.Max} 之間");
                }

                if (wave.Speed < rules.Validation.Speed.Min || wave.Speed > rules.Validation.Speed.Max)
                {
                    errors.Add($"等級 {level} 怪物速度必須在 {rules.Validation.Speed.Min}-{rules.Validation.Speed.Max} 之間");
                }
            }

            // 驗證獎勵表
            foreach (var (level, points) in rules.RewardTables.BasePointsPerLevel)
            {
                if (points < rules.Validation.BasePointsPerLevel.Min || 
                    points > rules.Validation.BasePointsPerLevel.Max)
                {
                    errors.Add($"等級 {level} 基礎點數必須在 {rules.Validation.BasePointsPerLevel.Min}-{rules.Validation.BasePointsPerLevel.Max} 之間");
                }
            }

            // 驗證時區
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(rules.GameRules.Timezone);
            }
            catch
            {
                errors.Add($"無效的時區設定: {rules.GameRules.Timezone}");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// 重新載入配置（清除快取）
        /// </summary>
        public async Task ReloadAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                _cachedRules = null;
                _lastReadTime = DateTime.MinValue;
                _logger.LogInformation("遊戲規則配置快取已清除");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <summary>
        /// 取得預設規則配置
        /// </summary>
        private static GameRulesOptions GetDefaultRules()
        {
            return new GameRulesOptions
            {
                Metadata = new MetadataOptions
                {
                    Version = "1.0.0",
                    Description = "預設遊戲規則配置",
                    Timezone = "Asia/Taipei"
                },
                GameRules = new GameRulesSettings
                {
                    DailyLimit = 3,
                    ResetTime = "00:00",
                    Timezone = "Asia/Taipei",
                    SessionTimeoutMinutes = 30
                },
                Validation = new ValidationOptions
                {
                    DailyLimit = new RangeOptions { Min = 1, Max = 10 },
                    BasePointsPerLevel = new RangeOptions { Min = 1, Max = 100 },
                    MonsterCount = new RangeOptions { Min = 1, Max = 50 },
                    Speed = new RangeDoubleOptions { Min = 0.1, Max = 5.0 }
                }
            };
        }

        /// <summary>
        /// 版本號遞增
        /// </summary>
        private static string IncrementVersion(string version)
        {
            if (Version.TryParse(version, out var ver))
            {
                return new Version(ver.Major, ver.Minor, ver.Build + 1).ToString();
            }
            return "1.0.1";
        }
    }
}