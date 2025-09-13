using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class UserWalletController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public UserWalletController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/UserWallet
        public async Task<IActionResult> Index()
        {
            // 取得目前用戶的錢包資訊
            var userId = GetCurrentUserId();
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                // 如果錢包不存在則建立
                wallet = new UserWallet
                {
                    UserId = userId,
                    UserPoint = 0
                };
                _context.UserWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            // 取得用戶持有的優惠券和電子券
            var coupons = await _context.Coupons
                .Where(c => c.UserId == userId && !c.IsUsed)
                .Include(c => c.CouponType)
                .ToListAsync();

            var evouchers = await _context.Evouchers
                .Where(e => e.UserId == userId && !e.IsUsed)
                .Include(e => e.EvoucherType)
                .ToListAsync();

            ViewBag.Coupons = coupons;
            ViewBag.EVouchers = evouchers;

            return View(wallet);
        }

        // GET: MiniGame/UserWallet/History
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            var history = await _context.WalletHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ChangeTime)
                .ToListAsync();

            return View(history);
        }

        // POST: MiniGame/UserWallet/AddPoints
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPoints(int points, string description = "手動新增點數")
        {
            if (points <= 0)
            {
                TempData["Error"] = "點數必須大於0";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 更新錢包餘額
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = userId, UserPoint = 0 };
                    _context.UserWallets.Add(wallet);
                }

                wallet.UserPoint += points;

                // 記錄錢包異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = points,
                    Description = description,
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功新增 {points} 點數！";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"操作失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: MiniGame/UserWallet/RedeemCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemCoupon(int couponTypeId)
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查優惠券類型
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null)
                {
                    TempData["Error"] = "無效的優惠券類型";
                    return RedirectToAction(nameof(Index));
                }

                // 檢查用戶錢包餘額
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                
                if (wallet == null || wallet.UserPoint < couponType.PointsCost)
                {
                    TempData["Error"] = "點數餘額不足";
                    return RedirectToAction(nameof(Index));
                }

                // 扣除點數
                wallet.UserPoint -= couponType.PointsCost;

                // 生成優惠券代碼
                var couponCode = GenerateCouponCode();
                
                // 建立優惠券
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };
                _context.Coupons.Add(coupon);

                // 記錄錢包異動
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = -couponType.PointsCost,
                    ItemCode = couponCode,
                    Description = $"兌換優惠券：{couponType.Name}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功兌換優惠券！券碼：{couponCode}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"兌換失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            // 暫時使用固定用戶ID進行測試
            // TODO: 實作適當的用戶身份驗證
            return 1;
        }

        private string GenerateCouponCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}