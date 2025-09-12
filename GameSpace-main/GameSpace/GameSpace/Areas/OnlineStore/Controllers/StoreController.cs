using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    [Authorize]
    public class StoreController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public StoreController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get available coupons and evouchers
            var coupons = await _context.Coupon
                .AsNoTracking()
                .Where(c => c.CouponStatus == "Active")
                .ToListAsync();

            var evouchers = await _context.Evoucher
                .AsNoTracking()
                .Where(e => e.EvoucherStatus == "Active")
                .ToListAsync();

            ViewBag.Coupons = coupons;
            ViewBag.Evouchers = evouchers;

            return View();
        }

        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupon
                .AsNoTracking()
                .Include(c => c.CouponType)
                .Where(c => c.CouponStatus == "Active")
                .ToListAsync();

            return View(coupons);
        }

        public async Task<IActionResult> Evouchers()
        {
            var evouchers = await _context.Evoucher
                .AsNoTracking()
                .Include(e => e.EvoucherType)
                .Where(e => e.EvoucherStatus == "Active")
                .ToListAsync();

            return View(evouchers);
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseCoupon(int couponId)
        {
            var userId = GetCurrentUserId();
            var coupon = await _context.Coupon.FindAsync(couponId);

            if (coupon == null || coupon.CouponStatus != "Active")
            {
                return BadRequest("Coupon not available");
            }

            // Check user's wallet balance
            var wallet = await _context.User_Wallet
                .FirstOrDefaultAsync(w => w.User_Id == userId);

            if (wallet == null || wallet.User_Point < coupon.CouponPrice)
            {
                return BadRequest("Insufficient points");
            }

            // Process purchase (placeholder implementation)
            wallet.User_Point -= coupon.CouponPrice;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Coupon purchased successfully" });
        }

        private int GetCurrentUserId()
        {
            // Placeholder - would get from authentication context
            return 1;
        }
    }
}