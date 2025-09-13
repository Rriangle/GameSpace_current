using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class WalletController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public WalletController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 錢包首頁
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            var coupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            var evouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            var walletHistories = await _context.WalletHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.ChangeTime)
                .Take(20)
                .ToListAsync();

            ViewBag.UserWallet = userWallet;
            ViewBag.Coupons = coupons;
            ViewBag.EVouchers = evouchers;
            ViewBag.WalletHistories = walletHistories;

            return View();
        }

        // 兌換優惠券
        [HttpPost]
        public async Task<IActionResult> RedeemCoupon(int couponTypeId)
        {
            var userId = GetCurrentUserId();
            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);

            if (couponType == null)
            {
                return Json(new { success = false, message = "找不到優惠券類型" });
            }

            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (userWallet == null)
            {
                return Json(new { success = false, message = "找不到錢包" });
            }

            if (userWallet.UserPoint < couponType.PointsCost)
            {
                return Json(new { success = false, message = "點數不足" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除點數
                userWallet.UserPoint -= couponType.PointsCost;

                // 創建優惠券
                var coupon = new Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    CouponCode = GenerateCouponCode(),
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };

                _context.Coupons.Add(coupon);

                // 記錄錢包歷史
                _context.WalletHistories.Add(new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = -couponType.PointsCost,
                    ItemCode = coupon.CouponCode,
                    Description = $"兌換優惠券：{couponType.Name}",
                    ChangeTime = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = $"兌換成功！獲得優惠券：{coupon.CouponCode}",
                    points = userWallet.UserPoint,
                    couponCode = coupon.CouponCode
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "兌換失敗" });
            }
        }

        // 兌換禮券
        [HttpPost]
        public async Task<IActionResult> RedeemEVoucher(int evoucherTypeId)
        {
            var userId = GetCurrentUserId();
            var evoucherType = await _context.EVoucherTypes
                .FirstOrDefaultAsync(et => et.EVoucherTypeId == evoucherTypeId);

            if (evoucherType == null)
            {
                return Json(new { success = false, message = "找不到禮券類型" });
            }

            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (userWallet == null)
            {
                return Json(new { success = false, message = "找不到錢包" });
            }

            if (userWallet.UserPoint < evoucherType.PointsCost)
            {
                return Json(new { success = false, message = "點數不足" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除點數
                userWallet.UserPoint -= evoucherType.PointsCost;

                // 創建禮券
                var evoucher = new EVoucher
                {
                    UserId = userId,
                    EVoucherTypeId = evoucherTypeId,
                    EVoucherCode = GenerateEVoucherCode(),
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };

                _context.EVouchers.Add(evoucher);

                // 記錄錢包歷史
                _context.WalletHistories.Add(new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "EVoucher",
                    PointsChanged = -evoucherType.PointsCost,
                    ItemCode = evoucher.EVoucherCode,
                    Description = $"兌換禮券：{evoucherType.Name}",
                    ChangeTime = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = $"兌換成功！獲得禮券：{evoucher.EVoucherCode}",
                    points = userWallet.UserPoint,
                    evoucherCode = evoucher.EVoucherCode
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "兌換失敗" });
            }
        }

        // 錢包歷史
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            var histories = await _context.WalletHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.ChangeTime)
                .ToListAsync();

            return View(histories);
        }

        // 優惠券列表
        public async Task<IActionResult> Coupons()
        {
            var userId = GetCurrentUserId();
            var coupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            return View(coupons);
        }

        // 禮券列表
        public async Task<IActionResult> EVouchers()
        {
            var userId = GetCurrentUserId();
            var evouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            return View(evouchers);
        }

        // 可兌換的優惠券類型
        public async Task<IActionResult> AvailableCoupons()
        {
            var couponTypes = await _context.CouponTypes
                .Where(ct => ct.ValidFrom <= DateTime.UtcNow && ct.ValidTo >= DateTime.UtcNow)
                .OrderBy(ct => ct.PointsCost)
                .ToListAsync();

            return View(couponTypes);
        }

        // 可兌換的禮券類型
        public async Task<IActionResult> AvailableEVouchers()
        {
            var evoucherTypes = await _context.EVoucherTypes
                .Where(et => et.ValidFrom <= DateTime.UtcNow && et.ValidTo >= DateTime.UtcNow)
                .OrderBy(et => et.PointsCost)
                .ToListAsync();

            return View(evoucherTypes);
        }

        private string GenerateCouponCode()
        {
            return $"COUPON{Random.Shared.Next(100000, 999999)}";
        }

        private string GenerateEVoucherCode()
        {
            return $"EVOUCHER{Random.Shared.Next(100000, 999999)}";
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}