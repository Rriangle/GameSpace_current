using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - UserSignInStats 模組後台管理控制器
    /// 負責管理會員簽到統計的後台功能
    /// 資料表範圍：UserSignInStats（以 database.json 為準）
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
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

            var totalCount = await query.AsNoTracking().CountAsync();
            var signInStats = await query
                .OrderByDescending(s => s.SignTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
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

            var totalCount = await query.AsNoTracking().CountAsync();
            var signInHistory = await query
                .OrderByDescending(s => s.SignTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // 計算連續簽到天數統計
            var allSignIns = await query
                .OrderByDescending(s => s.SignTime)
                .Select(s => s.SignTime.Date)
                .Distinct()
                .AsNoTracking()
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
                    TotalPoints = g.Sum(s => s.PointsGained),
                    TotalExp = g.Sum(s => s.ExpGained),
                    CouponsGiven = g.Count(s => s.CouponGained != "0")
                })
                .OrderBy(s => s.Date)
                .AsNoTracking()
                .ToListAsync();

            // 總體統計
            var totalSignIns = await query.AsNoTracking().CountAsync();
            var totalPoints = await query.AsNoTracking().SumAsync(s => s.PointsGained);
            var totalExp = await query.AsNoTracking().SumAsync(s => s.ExpGained);
            var totalCoupons = await query.AsNoTracking().CountAsync(s => s.CouponGained != "0");
            var uniqueUsers = await query.AsNoTracking().Select(s => s.UserID).Distinct().CountAsync();

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

        #region 規則展示和補簽/撤銷功能

        /// <summary>
        /// 簽到規則預覽頁面（唯讀）
        /// </summary>
        public IActionResult RulePreview()
        {
            // 從配置或常數讀取簽到規則
            var rules = new
            {
                DailyRewardPoints = 10, // 每日簽到獎勵點數
                DailyRewardExp = 5,     // 每日簽到獎勵經驗值
                StreakBonusThreshold = 7, // 連續簽到獎勵門檻
                StreakBonusPoints = 20,   // 連續簽到獎勵點數
                MaxDailySignIns = 1,      // 每日最大簽到次數
                ResetTime = "00:00 UTC",  // 重設時間
                ConfigSource = "程式常數", // 配置來源
                LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            ViewBag.Rules = rules;
            return View();
        }

        /// <summary>
        /// 顯示補簽/撤銷表單
        /// </summary>
        public async Task<IActionResult> Adjust(int? userId = null)
        {
            // 如果有指定 userId，載入該用戶資訊
            if (userId.HasValue)
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.User_ID == userId.Value);

                if (user != null)
                {
                    ViewBag.SelectedUser = user;
                    
                    // 載入該用戶最近的簽到記錄
                    var recentSignIns = await _context.UserSignInStats
                        .AsNoTracking()
                        .Where(s => s.UserID == userId.Value)
                        .OrderByDescending(s => s.SignTime)
                        .Take(10)
                        .ToListAsync();
                    
                    ViewBag.RecentSignIns = recentSignIns;
                }
            }

            return View();
        }

        /// <summary>
        /// 執行補簽/撤銷操作
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(string action, int userId, string reason, 
            DateTime? signDate = null, int? logId = null, int? points = null, int? exp = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "操作原因為必填";
                return RedirectToAction(nameof(Adjust), new { userId });
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (action == "AddSign")
                {
                    // 補簽操作
                    if (!signDate.HasValue)
                    {
                        TempData["Error"] = "補簽日期為必填";
                        return RedirectToAction(nameof(Adjust), new { userId });
                    }

                    // 檢查是否已有該日期的簽到記錄
                    var existingSign = await _context.UserSignInStats
                        .AsNoTracking()
                        .AnyAsync(s => s.UserID == userId && s.SignTime.Date == signDate.Value.Date);

                    if (existingSign)
                    {
                        TempData["Error"] = "該日期已有簽到記錄";
                        return RedirectToAction(nameof(Adjust), new { userId });
                    }

                    // 建立補簽記錄
                    var signInRecord = new UserSignInStats
                    {
                        UserID = userId,
                        SignTime = signDate.Value,
                        SignInDate = signDate.Value.Date,
                        PointsGained = points ?? 10, // 預設值
                        ExpGained = exp ?? 5,         // 預設值
                        PointsGainedTime = DateTime.UtcNow
                    };

                    _context.UserSignInStats.Add(signInRecord);

                    // 如果有點數影響，更新錢包並記錄歷史
                    if (signInRecord.PointsGained > 0)
                    {
                        var userWallet = await _context.UserWallets
                            .FirstOrDefaultAsync(w => w.UserID == userId);

                        if (userWallet != null)
                        {
                            userWallet.UserPoint += signInRecord.PointsGained;

                            var walletHistory = new WalletHistory
                            {
                                UserID = userId,
                                ChangeType = "ADMIN_SIGNIN",
                                PointsChanged = signInRecord.PointsGained,
                                ItemCode = "AdminAddSign",
                                Description = $"管理員補簽 - {reason}",
                                ChangeTime = DateTime.UtcNow
                            };

                            _context.WalletHistories.Add(walletHistory);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 記錄日誌
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminSignInStatsController>>();
                    logger.LogInformation("管理員補簽: UserId={UserId}, SignDate={SignDate}, Points={Points}, Exp={Exp}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                        userId, signDate.Value, signInRecord.PointsGained, signInRecord.ExpGained, reason, adminUserId, traceId);

                    TempData["Success"] = $"成功補簽 {signDate.Value:yyyy-MM-dd}，獲得 {signInRecord.PointsGained} 點數、{signInRecord.ExpGained} 經驗值";
                }
                else if (action == "Revoke")
                {
                    // 撤銷操作
                    if (!logId.HasValue)
                    {
                        TempData["Error"] = "請指定要撤銷的記錄 ID";
                        return RedirectToAction(nameof(Adjust), new { userId });
                    }

                    var signInRecord = await _context.UserSignInStats
                        .FirstOrDefaultAsync(s => s.LogID == logId.Value && s.UserID == userId);

                    if (signInRecord == null)
                    {
                        TempData["Error"] = "找不到指定的簽到記錄";
                        return RedirectToAction(nameof(Adjust), new { userId });
                    }

                    // 如果有點數影響，需要扣回並記錄
                    if (signInRecord.PointsGained > 0)
                    {
                        var userWallet = await _context.UserWallets
                            .FirstOrDefaultAsync(w => w.UserID == userId);

                        if (userWallet != null)
                        {
                            // 檢查點數是否足夠扣除
                            if (userWallet.UserPoint < signInRecord.PointsGained)
                            {
                                TempData["Error"] = $"用戶點數不足，無法撤銷（需扣除 {signInRecord.PointsGained} 點，目前僅有 {userWallet.UserPoint} 點）";
                                return RedirectToAction(nameof(Adjust), new { userId });
                            }

                            userWallet.UserPoint -= signInRecord.PointsGained;

                            var walletHistory = new WalletHistory
                            {
                                UserID = userId,
                                ChangeType = "ADMIN_REVOKE",
                                PointsChanged = -signInRecord.PointsGained,
                                ItemCode = "AdminRevokeSign",
                                Description = $"管理員撤銷簽到 - {reason}",
                                ChangeTime = DateTime.UtcNow
                            };

                            _context.WalletHistories.Add(walletHistory);
                        }
                    }

                    // 刪除簽到記錄
                    _context.UserSignInStats.Remove(signInRecord);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 記錄日誌
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminSignInStatsController>>();
                    logger.LogInformation("管理員撤銷簽到: UserId={UserId}, LogId={LogId}, Points={Points}, Exp={Exp}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                        userId, logId.Value, signInRecord.PointsGained, signInRecord.ExpGained, reason, adminUserId, traceId);

                    TempData["Success"] = $"成功撤銷簽到記錄，扣除 {signInRecord.PointsGained} 點數、{signInRecord.ExpGained} 經驗值";
                }
                else
                {
                    TempData["Error"] = "無效的操作類型";
                    return RedirectToAction(nameof(Adjust), new { userId });
                }

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminSignInStatsController>>();
                logger.LogError(ex, "簽到調整失敗: Action={Action}, UserId={UserId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    action, userId, reason, adminUserId, traceId);

                TempData["Error"] = "操作失敗：" + ex.Message;
                return RedirectToAction(nameof(Adjust), new { userId });
            }
        }

        #endregion

        #region 匯出功能和服務強化

        /// <summary>
        /// 簽到記錄 CSV 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportCsv(DateTime? startDate = null, DateTime? endDate = null, 
            int? userId = null, string userSearch = "")
        {
            var query = BuildSignInStatsQuery(startDate, endDate, userId, userSearch);
            
            var records = await query
                .OrderByDescending(s => s.SignTime)
                .AsNoTracking()
                .ToListAsync();

            var csvData = records.Select(s => new
            {
                簽到ID = s.LogID,
                會員ID = s.UserID,
                會員名稱 = s.User?.UserNickName ?? s.User?.UserName ?? "未知",
                簽到時間 = s.SignTime.ToString("yyyy-MM-dd HH:mm:ss"),
                簽到日期 = s.SignInDate.ToString("yyyy-MM-dd"),
                獲得點數 = s.PointsGained,
                獲得經驗 = s.ExpGained,
                點數發放時間 = s.PointsGainedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
            });

            return ExportService.CreateJsonFile(
                ExportService.CreateExportData(csvData, new {
                    查詢範圍 = $"{startDate?.ToString("yyyy-MM-dd") ?? "不限"} ~ {endDate?.ToString("yyyy-MM-dd") ?? "不限"}",
                    篩選條件 = new { 會員ID = userId?.ToString() ?? "全部", 搜尋關鍵字 = userSearch ?? "無" }
                }),
                "signin_stats"
            );
        }

        /// <summary>
        /// 簽到記錄 JSON 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportJson(DateTime? startDate = null, DateTime? endDate = null, 
            int? userId = null, string userSearch = "")
        {
            return await IndexExportCsv(startDate, endDate, userId, userSearch);
        }

        /// <summary>
        /// 建立簽到統計查詢（可重用）
        /// </summary>
        private IQueryable<UserSignInStats> BuildSignInStatsQuery(DateTime? startDate, DateTime? endDate, int? userId, string userSearch)
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

            if (!string.IsNullOrEmpty(userSearch))
            {
                query = query.Where(s => s.User.UserName.Contains(userSearch) || 
                                        s.User.UserNickName.Contains(userSearch));
            }

            return query;
        }

        /// <summary>
        /// 聚合 PointsGained 統計（修正服務方法）
        /// </summary>
        public async Task<object> GetPointsGainedAggregation(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var stats = await _context.UserSignInStats
                .Where(s => s.SignTime >= fromDate && s.SignTime <= toDate)
                .AsNoTracking()
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    SignInCount = g.Count(),
                    TotalPointsGained = g.Sum(s => s.PointsGained),
                    TotalExpGained = g.Sum(s => s.ExpGained),
                    AvgPointsPerSignIn = g.Average(s => s.PointsGained)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return new
            {
                Period = new { From = fromDate, To = toDate },
                DailyStats = stats,
                Summary = new
                {
                    TotalSignIns = stats.Sum(s => s.SignInCount),
                    TotalPointsGained = stats.Sum(s => s.TotalPointsGained),
                    TotalExpGained = stats.Sum(s => s.TotalExpGained),
                    AvgDailySignIns = stats.Any() ? stats.Average(s => s.SignInCount) : 0
                }
            };
        }

        #endregion
    }
}