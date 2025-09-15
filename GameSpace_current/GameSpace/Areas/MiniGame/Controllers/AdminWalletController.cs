using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - User_Wallet 模組後台管理控制器
    /// 負責管理會員錢包、點數、優惠券、電子禮券的後台功能
    /// 資料表範圍：User_Wallet, CouponType, Coupon, EVoucherType, EVoucher, EVoucherToken, EVoucherRedeemLog, WalletHistory
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    public class AdminWalletController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletController(GameSpaceDbContext context)
        {
            _context = context;
        }

        #region User_Wallet 管理

        /// <summary>
        /// 錢包列表頁面 - Read-first 原則
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string search = "")
        {
            var query = _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(w => w.User.UserName.Contains(search) || 
                                        w.User.UserNickName.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var wallets = await query
                .OrderByDescending(w => w.UserPoint)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(wallets);
        }

        /// <summary>
        /// 錢包明細頁面
        /// </summary>
        public async Task<IActionResult> Details(int userId)
        {
            var wallet = await _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                return NotFound("找不到指定的錢包資料");
            }

            // 取得錢包歷史記錄
            var histories = await _context.WalletHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ChangeTime)
                .Take(50)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Histories = histories;

            return View(wallet);
        }

        #endregion

        #region WalletHistory 管理

        /// <summary>
        /// 錢包歷史記錄列表
        /// </summary>
        public async Task<IActionResult> History(int page = 1, int pageSize = 50, int? userId = null, string changeType = "")
        {
            var query = _context.WalletHistories
                .Include(h => h.User)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(h => h.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(changeType))
            {
                query = query.Where(h => h.ChangeType == changeType);
            }

            var totalCount = await query.CountAsync();
            var histories = await query
                .OrderByDescending(h => h.ChangeTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.UserId = userId;
            ViewBag.ChangeType = changeType;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(histories);
        }

        #endregion

        #region EVoucher 管理

        /// <summary>
        /// 電子禮券列表頁面
        /// </summary>
        public async Task<IActionResult> EVouchers(int page = 1, int pageSize = 20, bool? isUsed = null, int? typeId = null)
        {
            var query = _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .AsNoTracking();

            if (isUsed.HasValue)
            {
                query = query.Where(e => e.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(e => e.EVoucherTypeID == typeId.Value);
            }

            var totalCount = await query.CountAsync();
            var evouchers = await query
                .OrderByDescending(e => e.AcquiredTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 取得所有禮券類型供篩選使用
            var evoucherTypes = await _context.EVoucherTypes
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.IsUsed = isUsed;
            ViewBag.TypeId = typeId;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.EVoucherTypes = evoucherTypes;

            return View(evouchers);
        }

        /// <summary>
        /// 電子禮券明細頁面
        /// </summary>
        public async Task<IActionResult> EVoucherDetails(int id)
        {
            var evoucher = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EVoucherID == id);

            if (evoucher == null)
            {
                return NotFound("找不到指定的電子禮券");
            }

            // 取得相關的 Token 和核銷記錄
            var tokens = await _context.EVoucherTokens
                .Where(t => t.EVoucherID == id)
                .AsNoTracking()
                .ToListAsync();

            var redeemLogs = await _context.EVoucherRedeemLogs
                .Where(r => r.EVoucherID == id)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Tokens = tokens;
            ViewBag.RedeemLogs = redeemLogs;

            return View(evoucher);
        }

        #endregion

        #region Coupon 管理

        /// <summary>
        /// 優惠券列表頁面
        /// </summary>
        public async Task<IActionResult> Coupons(int page = 1, int pageSize = 20, bool? isUsed = null, int? typeId = null)
        {
            var query = _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .AsNoTracking();

            if (isUsed.HasValue)
            {
                query = query.Where(c => c.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(c => c.CouponTypeID == typeId.Value);
            }

            var totalCount = await query.CountAsync();
            var coupons = await query
                .OrderByDescending(c => c.AcquiredTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 取得所有優惠券類型供篩選使用
            var couponTypes = await _context.CouponTypes
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.IsUsed = isUsed;
            ViewBag.TypeId = typeId;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CouponTypes = couponTypes;

            return View(coupons);
        }

        #endregion
    }
}