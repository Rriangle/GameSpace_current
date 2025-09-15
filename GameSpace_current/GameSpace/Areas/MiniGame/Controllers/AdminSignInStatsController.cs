using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - UserSignInStats 模組後台管理控制器
    /// 負責管理會員簽到統計的後台功能
    /// 資料表範圍：UserSignInStats（以 database.json 為準）
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    public class AdminSignInStatsController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminSignInStatsController(GameSpaceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 簽到統計列表頁面 - Read-first 原則
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, 
            DateTime? startDate = null, DateTime? endDate = null, 
            int? userId = null, string userSearch = "")
        {
            var query = _context.UserSignInStats
                .Include(s => s.User)
                .AsNoTracking();

            // 日期篩選
            if (startDate.HasValue)
            {
                query = query.Where(s => s.SignTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SignTime <= endDate.Value.AddDays(1));
            }

            // 用戶篩選
            if (userId.HasValue)
            {
                query = query.Where(s => s.UserID == userId.Value);
            }
            else if (!string.IsNullOrEmpty(userSearch))
            {
                query = query.Where(s => s.User.UserName.Contains(userSearch) || 
                                        s.User.UserNickName.Contains(userSearch));
            }

            var totalCount = await query.CountAsync();
            var signInStats = await query
                .OrderByDescending(s => s.SignTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.UserId = userId;
            ViewBag.UserSearch = userSearch;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(signInStats);
        }

        /// <summary>
        /// 簽到統計明細頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var signInStat = await _context.UserSignInStats
                .Include(s => s.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.LogID == id);

            if (signInStat == null)
            {
                return NotFound("找不到指定的簽到記錄");
            }

            return View(signInStat);
        }

        /// <summary>
        /// 用戶簽到歷史頁面
        /// </summary>
        public async Task<IActionResult> UserHistory(int userId, int page = 1, int pageSize = 30)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound("找不到指定的用戶");
            }

            var query = _context.UserSignInStats
                .Where(s => s.UserID == userId)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var signInHistory = await query
                .OrderByDescending(s => s.SignTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 計算連續簽到天數統計
            var allSignIns = await query
                .OrderByDescending(s => s.SignTime)
                .Select(s => s.SignTime.Date)
                .Distinct()
                .ToListAsync();

            var currentStreak = CalculateCurrentStreak(allSignIns);
            var longestStreak = CalculateLongestStreak(allSignIns);

            ViewBag.User = user;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CurrentStreak = currentStreak;
            ViewBag.LongestStreak = longestStreak;
            ViewBag.TotalSignInDays = allSignIns.Count;

            return View(signInHistory);
        }

        /// <summary>
        /// 簽到統計報表頁面
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

            var query = _context.UserSignInStats
                .Where(s => s.SignTime >= startDate.Value && s.SignTime <= endDate.Value.AddDays(1))
                .AsNoTracking();

            // 每日簽到統計
            var dailyStats = await query
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    TotalPoints = g.Sum(s => s.PointsChanged),
                    TotalExp = g.Sum(s => s.ExpGained),
                    CouponsGiven = g.Count(s => s.CouponGained != "0")
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            // 總體統計
            var totalSignIns = await query.CountAsync();
            var totalPoints = await query.SumAsync(s => s.PointsChanged);
            var totalExp = await query.SumAsync(s => s.ExpGained);
            var totalCoupons = await query.CountAsync(s => s.CouponGained != "0");
            var uniqueUsers = await query.Select(s => s.UserID).Distinct().CountAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.DailyStats = dailyStats;
            ViewBag.TotalSignIns = totalSignIns;
            ViewBag.TotalPoints = totalPoints;
            ViewBag.TotalExp = totalExp;
            ViewBag.TotalCoupons = totalCoupons;
            ViewBag.UniqueUsers = uniqueUsers;

            return View();
        }

        #region Helper Methods

        /// <summary>
        /// 計算當前連續簽到天數
        /// </summary>
        private int CalculateCurrentStreak(List<DateTime> signInDates)
        {
            if (!signInDates.Any()) return 0;

            var today = DateTime.Today;
            var streak = 0;

            // 檢查今天是否簽到，如果沒有則從昨天開始算
            var checkDate = signInDates.Contains(today) ? today : today.AddDays(-1);

            foreach (var date in signInDates.OrderByDescending(d => d))
            {
                if (date == checkDate)
                {
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        /// <summary>
        /// 計算最長連續簽到天數
        /// </summary>
        private int CalculateLongestStreak(List<DateTime> signInDates)
        {
            if (!signInDates.Any()) return 0;

            var sortedDates = signInDates.OrderBy(d => d).ToList();
            var longestStreak = 1;
            var currentStreak = 1;

            for (int i = 1; i < sortedDates.Count; i++)
            {
                if (sortedDates[i] == sortedDates[i - 1].AddDays(1))
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }

        #endregion
    }
}