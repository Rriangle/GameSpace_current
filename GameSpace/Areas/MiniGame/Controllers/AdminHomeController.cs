using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area Admin 首頁控制器
    /// 提供儀表板和總覽功能
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
    public class AdminHomeController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminHomeController(GameSpaceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// MiniGame Admin 儀表板
        /// 顯示關鍵即時指標和診斷資訊
        /// </summary>
        public async Task<IActionResult> Dashboard(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                // 預設最近 14 天
                var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                var toDate = to ?? DateTime.UtcNow;
                var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                // 計算 5 個輕量級計數（避免重型查詢，全部使用 AsNoTracking）
                
                // 1. 錢包異動記錄（區間內）
                var walletHistoryCount = await _context.WalletHistories
                    .Where(h => h.ChangeTime >= fromDate && h.ChangeTime <= toDateEndOfDay)
                    .AsNoTracking()
                    .CountAsync();

                // 2. EVoucherToken 總數
                var evoucherTokenTotalCount = await _context.EVoucherTokens
                    .AsNoTracking()
                    .CountAsync();

                // 3. 兌換記錄（區間內）
                var redeemLogCount = await _context.EVoucherRedeemLogs
                    .Where(r => r.ScannedAt >= fromDate && r.ScannedAt <= toDateEndOfDay)
                    .AsNoTracking()
                    .CountAsync();

                // 4. 簽到記錄（區間內）
                var signInStatsCount = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date >= fromDate.Date && s.SignTime.Date <= toDate.Date)
                    .AsNoTracking()
                    .CountAsync();

                // 5. MiniGame 場次（區間內）
                var miniGameCount = await _context.MiniGames
                    .Where(m => m.PointsGainedTime >= fromDate && m.PointsGainedTime <= toDateEndOfDay)
                    .AsNoTracking()
                    .CountAsync();

                // 設定 ViewBag 資料
                ViewBag.From = fromDate;
                ViewBag.To = toDate;
                ViewBag.WalletHistoryCount = walletHistoryCount;
                ViewBag.EVoucherTokenTotalCount = evoucherTokenTotalCount;
                ViewBag.RedeemLogCount = redeemLogCount;
                ViewBag.SignInStatsCount = signInStatsCount;
                ViewBag.MiniGameCount = miniGameCount;

                // 計算一些簡單的衍生指標
                ViewBag.DateRangeText = $"{fromDate:yyyy/MM/dd} - {toDate:yyyy/MM/dd}";
                ViewBag.DaysSpan = (toDate.Date - fromDate.Date).Days + 1;
                ViewBag.AvgSignInsPerDay = ViewBag.DaysSpan > 0 ? Math.Round((double)signInStatsCount / ViewBag.DaysSpan, 1) : 0;
                ViewBag.AvgGamesPerDay = ViewBag.DaysSpan > 0 ? Math.Round((double)miniGameCount / ViewBag.DaysSpan, 1) : 0;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"儀表板資料載入失敗：{ex.Message}";
                return View();
            }
        }
    }
}