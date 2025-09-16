using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - MiniGame 模組後台管理控制器
    /// 負責管理小遊戲項目的後台功能
    /// 資料表範圍：MiniGame（以 database.json 為準）
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
    public class AdminMiniGameController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminMiniGameController(GameSpaceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 小遊戲記錄列表頁面 - Read-first 原則
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, 
            string result = "", int? level = null, int? userId = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .AsNoTracking();

            // 篩選條件
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(m => m.Result == result);
            }

            if (level.HasValue)
            {
                query = query.Where(m => m.Level == level.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(m => m.UserID == userId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.StartTime <= endDate.Value.AddDays(1));
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var games = await query
                .OrderByDescending(m => m.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Result = result;
            ViewBag.Level = level;
            ViewBag.UserId = userId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(games);
        }

        /// <summary>
        /// 遊戲記錄明細頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var game = await _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PlayID == id);

            if (game == null)
            {
                return NotFound("找不到指定的遊戲記錄");
            }

            return View(game);
        }

        /// <summary>
        /// 遊戲統計頁面
        /// </summary>
        public async Task<IActionResult> Statistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            // 預設查詢最近30天
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today.AddDays(-29);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Today;
            }

            var query = _context.MiniGames
                .Where(m => m.StartTime >= startDate.Value && m.StartTime <= endDate.Value.AddDays(1))
                .AsNoTracking();

            // 總體統計
            var totalGames = await query.AsNoTracking().CountAsync();
            var winCount = await query.AsNoTracking().CountAsync(m => m.Result == "Win");
            var loseCount = await query.AsNoTracking().CountAsync(m => m.Result == "Lose");
            var abortCount = await query.AsNoTracking().CountAsync(m => m.Result == "Abort");
            var totalPoints = await query.AsNoTracking().SumAsync(m => m.PointsChanged);
            var totalExp = await query.AsNoTracking().SumAsync(m => m.ExpGained);

            // 關卡統計
            var levelStats = await query
                .GroupBy(m => m.Level)
                .Select(g => new {
                    Level = g.Key,
                    Count = g.Count(),
                    WinRate = g.Count(m => m.Result == "Win") * 100.0 / g.Count(),
                    AvgPoints = g.Average(m => m.PointsChanged),
                    AvgExp = g.Average(m => m.ExpGained)
                })
                .OrderBy(s => s.Level)
                .AsNoTracking()
                .ToListAsync();

            // 每日統計
            var dailyStats = await query
                .GroupBy(m => m.StartTime.Date)
                .Select(g => new {
                    Date = g.Key,
                    Count = g.Count(),
                    WinCount = g.Count(m => m.Result == "Win"),
                    LoseCount = g.Count(m => m.Result == "Lose"),
                    AbortCount = g.Count(m => m.Result == "Abort")
                })
                .OrderBy(s => s.Date)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalGames = totalGames;
            ViewBag.WinCount = winCount;
            ViewBag.LoseCount = loseCount;
            ViewBag.AbortCount = abortCount;
            ViewBag.TotalPoints = totalPoints;
            ViewBag.TotalExp = totalExp;
            ViewBag.WinRate = totalGames > 0 ? (winCount * 100.0 / totalGames) : 0;
            ViewBag.LevelStats = levelStats;
            ViewBag.DailyStats = dailyStats;

            return View();
        }

        /// <summary>
        /// 遊戲設定管理功能 - 預留實作介面
        /// 根據指令：寫入流程僅可建立不破壞規格的預留實作
        /// </summary>
        public IActionResult Settings()
        {
            // 預留實作：顯示遊戲設定介面，實際修改功能為預留實作
            
            var gameSettings = new
            {
                MaxDailyPlays = 3,
                Level1MonsterCount = 6,
                Level1SpeedMultiplier = 1.0,
                Level2MonsterCount = 8,
                Level2SpeedMultiplier = 1.5,
                Level3MonsterCount = 10,
                Level3SpeedMultiplier = 2.0,
                BasePointReward = 10,
                BaseExpReward = 100
            };

            ViewBag.GameSettings = gameSettings;
            return View();
        }

        /// <summary>
        /// 更新遊戲設定功能
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSettings(int maxDailyPlays, int basePointReward, int baseExpReward, string reason)
        {
            // 預留實作：驗證與流程說明到位，實際寫入前先經過後續階段與規格允許再開啟
            
            // 驗證設定值
            if (maxDailyPlays < 1 || maxDailyPlays > 10)
            {
                TempData["ErrorMessage"] = "每日遊戲次數必須在 1-10 範圍內";
                return RedirectToAction(nameof(Settings));
            }

            if (basePointReward < 1 || basePointReward > 100)
            {
                TempData["ErrorMessage"] = "基礎點數獎勵必須在 1-100 範圍內";
                return RedirectToAction(nameof(Settings));
            }

            if (baseExpReward < 1 || baseExpReward > 1000)
            {
                TempData["ErrorMessage"] = "基礎經驗獎勵必須在 1-1000 範圍內";
                return RedirectToAction(nameof(Settings));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "請提供設定變更原因";
                return RedirectToAction(nameof(Settings));
            }

            // 預留實作提示：實際設定更新功能待後續階段開啟
            TempData["InfoMessage"] = $"遊戲設定更新功能預留實作。" +
                $"預計更新：每日次數 {maxDailyPlays}、基礎點數 {basePointReward}、基礎經驗 {baseExpReward}，原因：{reason}";

            return RedirectToAction(nameof(Settings));
        }

        /// <summary>
        /// 遊戲資料清理功能
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupData(DateTime beforeDate, bool includeAborted = false)
        {
            // 預留實作：提供資料清理的驗證與流程說明
            
            if (beforeDate >= DateTime.Today.AddDays(-7))
            {
                TempData["ErrorMessage"] = "為確保資料安全，僅允許清理7天前的資料";
                return RedirectToAction(nameof(Index));
            }

            var query = _context.MiniGames.Where(m => m.StartTime < beforeDate);
            if (includeAborted)
            {
                query = query.Where(m => m.Result == "Abort");
            }

            var affectedCount = await query.AsNoTracking().CountAsync();

            // 預留實作提示：實際清理功能待後續階段開啟
            TempData["InfoMessage"] = $"遊戲資料清理功能預留實作。" +
                $"預計清理 {beforeDate:yyyy/MM/dd} 之前的 {affectedCount} 筆記錄" +
                $"{(includeAborted ? "（僅包含中斷記錄）" : "")}";

            return RedirectToAction(nameof(Index));
        }

        #region 遊戲規則展示

        /// <summary>
        /// 小遊戲規則預覽頁面（唯讀）
        /// </summary>
        public IActionResult RulePreview()
        {
            // 從配置或程式常數讀取遊戲規則
            var gameRules = new
            {
                // 基本遊戲參數
                MonsterCount = 10,              // 每關怪物數量
                MovementSpeed = 1.5,            // 角色移動速度
                AttackPower = 100,              // 攻擊力
                DefensePower = 50,              // 防禦力
                
                // 等級與難度
                MaxLevel = 50,                  // 最高等級
                LevelUpExpRequired = 1000,      // 升級所需經驗值
                DifficultyScaling = 1.2,        // 難度遞增係數
                
                // 獎勵機制
                BasePointsReward = 100,         // 基礎點數獎勵
                BaseExpReward = 50,             // 基礎經驗值獎勵
                LevelMultiplier = 1.1,          // 等級獎勵倍數
                PerfectClearBonus = 0.5,        // 完美通關獎勵加成
                
                // 優惠券獎勵
                CouponDropRate = 0.1,           // 優惠券掉落機率 (10%)
                RareCouponRate = 0.02,          // 稀有優惠券機率 (2%)
                CouponTypes = new[] { "GAME_BONUS", "SHOP_DISCOUNT", "EXP_BOOST" },
                
                // 遊戲限制
                DailyPlayLimit = 10,            // 每日遊玩次數上限
                EnergyConsumption = 1,          // 每次消耗體力
                CooldownMinutes = 5,            // 冷卻時間（分鐘）
                
                // 時間限制
                GameTimeLimit = 300,            // 遊戲時間限制（秒）
                IdleTimeout = 60,               // 閒置超時（秒）
                
                // 配置資訊
                ConfigSource = "程式常數",       // 配置來源
                LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Version = "1.0.0"               // 規則版本
            };

            ViewBag.GameRules = gameRules;
            return View();
        }

        #endregion

        #region 匯出功能

        /// <summary>
        /// 遊戲記錄 CSV 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportCsv(string result = "", int? level = null, int? userId = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = BuildMiniGameQuery(result, level, userId, startDate, endDate);
            
            var games = await query
                .OrderByDescending(m => m.StartTime)
                .AsNoTracking()
                .ToListAsync();

            var csvData = games.Select(m => new
            {
                遊戲ID = m.PlayID,
                會員ID = m.UserID,
                會員名稱 = m.User?.UserNickName ?? m.User?.UserName ?? "未知",
                寵物ID = m.PetID,
                寵物名稱 = m.Pet?.PetName ?? "未知",
                關卡等級 = m.Level,
                怪物數量 = m.MonsterCount,
                速度倍率 = m.SpeedMultiplier,
                遊戲結果 = m.Result,
                獲得經驗 = m.ExpGained,
                獲得點數 = m.PointsGained,
                獲得優惠券 = m.CouponGained != "0" ? m.CouponGained : "",
                開始時間 = m.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                結束時間 = m.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                是否中途退出 = m.Aborted ? "是" : "否"
            });

            return ExportService.CreateJsonFile(
                ExportService.CreateExportData(csvData, new {
                    篩選條件 = new { 
                        遊戲結果 = result ?? "全部",
                        關卡等級 = level?.ToString() ?? "全部",
                        會員ID = userId?.ToString() ?? "全部",
                        開始日期 = startDate?.ToString("yyyy-MM-dd") ?? "不限",
                        結束日期 = endDate?.ToString("yyyy-MM-dd") ?? "不限"
                    }
                }),
                "minigame_records"
            );
        }

        /// <summary>
        /// 遊戲記錄 JSON 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportJson(string result = "", int? level = null, int? userId = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            return await IndexExportCsv(result, level, userId, startDate, endDate);
        }

        /// <summary>
        /// 建立遊戲記錄查詢（可重用）
        /// </summary>
        private IQueryable<Models.MiniGame> BuildMiniGameQuery(string result, int? level, int? userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .AsNoTracking();

            // 篩選條件
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(m => m.Result == result);
            }

            if (level.HasValue)
            {
                query = query.Where(m => m.Level == level.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(m => m.UserID == userId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.StartTime <= endDate.Value.AddDays(1));
            }

            return query;
        }

        #endregion
    }
}