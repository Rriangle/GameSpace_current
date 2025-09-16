using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Admin 遊戲規則管理控制器
    /// 提供遊戲規則的查看和編輯功能
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    [MiniGameAdminAuthorize]
    public class AdminMiniGameRulesController : Controller
    {
        private readonly IGameRulesStore _gameRulesStore;
        private readonly ILogger<AdminMiniGameRulesController> _logger;

        public AdminMiniGameRulesController(IGameRulesStore gameRulesStore, ILogger<AdminMiniGameRulesController> logger)
        {
            _gameRulesStore = gameRulesStore;
            _logger = logger;
        }

        /// <summary>
        /// 顯示當前遊戲規則配置
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var rules = await _gameRulesStore.GetRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入遊戲規則配置失敗");
                TempData["Error"] = "載入遊戲規則配置失敗：" + ex.Message;
                return View(new GameRulesOptions());
            }
        }

        /// <summary>
        /// 顯示編輯遊戲規則表單
        /// </summary>
        public async Task<IActionResult> Edit()
        {
            try
            {
                var rules = await _gameRulesStore.GetRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入遊戲規則配置失敗");
                TempData["Error"] = "載入遊戲規則配置失敗：" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 保存遊戲規則配置
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameRulesOptions rules)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "表單驗證失敗，請檢查輸入內容";
                return View(rules);
            }

            try
            {
                // 驗證規則配置
                var (isValid, errors) = await _gameRulesStore.ValidateRulesAsync(rules);
                if (!isValid)
                {
                    TempData["Error"] = "規則驗證失敗：" + string.Join("；", errors);
                    return View(rules);
                }

                // 保存配置
                await _gameRulesStore.SaveRulesAsync(rules);

                // 記錄 Serilog 審計
                _logger.LogInformation("Admin 更新遊戲規則配置: DailyLimit={DailyLimit}, Version={Version}, TraceID={TraceID}",
                    rules.GameRules.DailyLimit, rules.Metadata.Version, HttpContext.TraceIdentifier);

                TempData["Success"] = $"遊戲規則配置已成功更新（版本 {rules.Metadata.Version}）";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存遊戲規則配置失敗: TraceID={TraceID}", HttpContext.TraceIdentifier);
                TempData["Error"] = "保存遊戲規則配置失敗：" + ex.Message;
                return View(rules);
            }
        }

        /// <summary>
        /// 重新載入配置（清除快取）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reload()
        {
            try
            {
                await _gameRulesStore.ReloadAsync();
                
                _logger.LogInformation("Admin 重新載入遊戲規則配置: TraceID={TraceID}", HttpContext.TraceIdentifier);
                
                TempData["Success"] = "遊戲規則配置已重新載入";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新載入遊戲規則配置失敗: TraceID={TraceID}", HttpContext.TraceIdentifier);
                TempData["Error"] = "重新載入遊戲規則配置失敗：" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 恢復預設配置
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDefaults()
        {
            try
            {
                var defaultRules = new GameRulesOptions
                {
                    Metadata = new MetadataOptions
                    {
                        Version = "1.0.0",
                        Description = "恢復預設遊戲規則配置",
                        Timezone = "Asia/Taipei"
                    },
                    GameRules = new GameRulesSettings
                    {
                        DailyLimit = 3,
                        ResetTime = "00:00",
                        Timezone = "Asia/Taipei",
                        SessionTimeoutMinutes = 30
                    },
                    RewardTables = new RewardTablesOptions
                    {
                        BasePointsPerLevel = new Dictionary<string, int>
                        {
                            ["1"] = 10, ["2"] = 15, ["3"] = 20, ["4"] = 25, ["5"] = 30
                        },
                        ExpPerLevel = new Dictionary<string, int>
                        {
                            ["1"] = 5, ["2"] = 8, ["3"] = 12, ["4"] = 15, ["5"] = 20
                        },
                        Multipliers = new MultiplierOptions
                        {
                            Win = 1.5, Lose = 0.5, Abort = 0.0
                        }
                    },
                    MonsterWaves = new Dictionary<string, MonsterWaveOptions>
                    {
                        ["level1"] = new() { MonsterCount = 3, Speed = 1.0, Difficulty = "easy" },
                        ["level2"] = new() { MonsterCount = 5, Speed = 1.2, Difficulty = "normal" },
                        ["level3"] = new() { MonsterCount = 7, Speed = 1.5, Difficulty = "hard" },
                        ["level4"] = new() { MonsterCount = 10, Speed = 1.8, Difficulty = "expert" },
                        ["level5"] = new() { MonsterCount = 15, Speed = 2.0, Difficulty = "master" }
                    },
                    Validation = new ValidationOptions
                    {
                        DailyLimit = new RangeOptions { Min = 1, Max = 10 },
                        BasePointsPerLevel = new RangeOptions { Min = 1, Max = 100 },
                        MonsterCount = new RangeOptions { Min = 1, Max = 50 },
                        Speed = new RangeDoubleOptions { Min = 0.1, Max = 5.0 }
                    }
                };

                await _gameRulesStore.SaveRulesAsync(defaultRules);

                _logger.LogInformation("Admin 恢復預設遊戲規則配置: TraceID={TraceID}", HttpContext.TraceIdentifier);

                TempData["Success"] = "遊戲規則配置已恢復為預設值";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "恢復預設遊戲規則配置失敗: TraceID={TraceID}", HttpContext.TraceIdentifier);
                TempData["Error"] = "恢復預設遊戲規則配置失敗：" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API 端點：取得當前遊戲規則（供前端 AJAX 使用）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRulesJson()
        {
            try
            {
                var rules = await _gameRulesStore.GetRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得遊戲規則 JSON 失敗");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}