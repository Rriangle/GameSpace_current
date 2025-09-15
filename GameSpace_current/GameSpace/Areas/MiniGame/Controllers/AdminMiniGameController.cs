using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - MiniGame 模組後台管理控制器
    /// 負責管理小遊戲項目的後台功能
    /// 資料表範圍：MiniGame（以 database.json 為準）
    /// 根據指令第[4]節：其餘表以後台管理的查詢與審閱頁為主，寫入功能為預留實作
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
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

            var totalCount = await query.CountAsync();
            var games = await query
                .OrderByDescending(m => m.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
            var totalGames = await query.CountAsync();
            var winCount = await query.CountAsync(m => m.Result == "Win");
            var loseCount = await query.CountAsync(m => m.Result == "Lose");
            var abortCount = await query.CountAsync(m => m.Result == "Abort");
            var totalPoints = await query.SumAsync(m => m.PointsChanged);
            var totalExp = await query.SumAsync(m => m.ExpGained);

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

            var affectedCount = await query.CountAsync();

            // 預留實作提示：實際清理功能待後續階段開啟
            TempData["InfoMessage"] = $"遊戲資料清理功能預留實作。" +
                $"預計清理 {beforeDate:yyyy/MM/dd} 之前的 {affectedCount} 筆記錄" +
                $"{(includeAborted ? "（僅包含中斷記錄）" : "")}";

            return RedirectToAction(nameof(Index));
        }
    }
}