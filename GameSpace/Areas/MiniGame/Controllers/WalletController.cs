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
            var userId = GetCurrentUserID();
            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            var coupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            var evouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserID == userId)
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            var walletHistories = await _context.WalletHistories
                .Where(w => w.UserID == userId)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemCoupon(int couponTypeId)
        {
            var userId = GetCurrentUserID();
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
                    RelatedID = coupon.CouponID, // 關聯到優惠券ID以確保DB欄位100%覆蓋
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemEVoucher(int evoucherTypeId)
        {
            var userId = GetCurrentUserID();
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
                    RelatedID = evoucher.EVoucherID, // 關聯到電子券ID以確保DB欄位100%覆蓋
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
            var userId = GetCurrentUserID();
            var histories = await _context.WalletHistories
                .Where(w => w.UserID == userId)
                .OrderByDescending(w => w.ChangeTime)
                .ToListAsync();

            return View(histories);
        }

        // 優惠券列表
        public async Task<IActionResult> Coupons()
        {
            var userId = GetCurrentUserID();
            var coupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            return View(coupons);
        }

        // 禮券列表
        public async Task<IActionResult> EVouchers()
        {
            var userId = GetCurrentUserID();
            var evouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserID == userId)
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

        /// <summary>
        /// 取得當前登入會員 ID
        /// </summary>
        private int GetCurrentUserID()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst("sub") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userID))
                {
                    return userID;
                }
            }
            throw new UnauthorizedAccessException("無法取得會員身份資訊");
        }

        #region EVoucher QR/Barcode 與核銷功能

        /// <summary>
        /// 顯示電子禮券 QR Code / Barcode 出示畫面
        /// </summary>
        public async Task<IActionResult> ShowEVoucher(int id)
        {
            var userId = GetCurrentUserID();
            
            var evoucher = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EVoucherID == id && e.UserID == userId);

            if (evoucher == null)
            {
                return NotFound("找不到指定的電子禮券");
            }

            if (evoucher.IsUsed)
            {
                TempData["Error"] = "此電子禮券已使用過";
                return RedirectToAction(nameof(EVouchers));
            }

            // 生成一次性 Token 用於 QR Code（60秒防重機制）
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cutoffTime = DateTime.UtcNow.AddSeconds(-60);
                
                // 檢查是否已有未過期的 Token
                var existingToken = await _context.EVoucherTokens
                    .Where(t => t.EVoucherID == id && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                EVoucherToken token;
                if (existingToken != null)
                {
                    token = existingToken;
                }
                else
                {
                    // 建立新的一次性 Token（5分鐘有效）
                    token = new EVoucherToken
                    {
                        EVoucherID = id,
                        Token = Guid.NewGuid().ToString("N")[..16].ToUpper(), // 16字符 Token
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                        IsRevoked = false
                    };

                    _context.EVoucherTokens.Add(token);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                ViewBag.EVoucher = evoucher;
                ViewBag.Token = token;
                
                // 記錄出示行為
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogInformation("會員出示電子禮券: EVoucherID={EVoucherID}, TokenID={TokenID}, UserID={UserID}, ExpiresAt={ExpiresAt}",
                    id, token.TokenID, userId, token.ExpiresAt);

                return View();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogError(ex, "生成 EVoucher Token 失敗: EVoucherID={EVoucherID}, UserID={UserID}", id, userId);
                TempData["Error"] = "無法生成兌換碼，請稍後再試";
                return RedirectToAction(nameof(EVouchers));
            }
        }

        /// <summary>
        /// 核銷電子禮券（通常由店員或系統呼叫）
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedeemEVoucher(string token, string location = "")
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Json(new { success = false, message = "Token 不能為空" });
            }

            var userId = GetCurrentUserID();
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 60秒防重檢查
                var cutoffTime = DateTime.UtcNow.AddSeconds(-60);
                var duplicateCheck = await _context.EVoucherRedeemLogs
                    .AsNoTracking()
                    .AnyAsync(r => r.Token == token && 
                                 r.ScannedAt >= cutoffTime && 
                                 r.Status == "Approved");

                if (duplicateCheck)
                {
                    return Json(new { success = false, message = "60秒內已有相同的核銷記錄" });
                }

                // 查找 Token
                var evoucherToken = await _context.EVoucherTokens
                    .Include(t => t.EVoucher)
                    .ThenInclude(e => e.EVoucherType)
                    .FirstOrDefaultAsync(t => t.Token == token);

                string status = "Rejected";
                string message = "核銷失敗";

                if (evoucherToken == null)
                {
                    status = "Rejected";
                    message = "無效的兌換碼";
                }
                else if (evoucherToken.IsRevoked)
                {
                    status = "Revoked";
                    message = "兌換碼已撤銷";
                }
                else if (evoucherToken.ExpiresAt <= DateTime.UtcNow)
                {
                    status = "Expired";
                    message = "兌換碼已過期";
                }
                else if (evoucherToken.EVoucher.IsUsed)
                {
                    status = "AlreadyUsed";
                    message = "電子禮券已使用過";
                }
                else
                {
                    // 核銷成功
                    evoucherToken.EVoucher.IsUsed = true;
                    evoucherToken.EVoucher.UsedTime = DateTime.UtcNow;
                    evoucherToken.IsRevoked = true; // Token 一次性使用

                    status = "Approved";
                    message = $"核銷成功：{evoucherToken.EVoucher.EVoucherType.TypeName}";
                }

                // 記錄核銷日誌
                var redeemLog = new EVoucherRedeemLog
                {
                    EVoucherID = evoucherToken?.EVoucherID ?? 0,
                    TokenID = evoucherToken?.TokenID,
                    Token = token,
                    UserID = userId,
                    ScannedAt = DateTime.UtcNow,
                    Status = status,
                    Location = location ?? ""
                };

                _context.EVoucherRedeemLogs.Add(redeemLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄 Serilog
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogInformation("電子禮券核銷: Token={Token}, EVoucherID={EVoucherID}, UserID={UserID}, Status={Status}, Location={Location}, TraceID={TraceID}",
                    token, evoucherToken?.EVoucherID ?? 0, userId, status, location, traceId);

                return Json(new { 
                    success = status == "Approved", 
                    message = message,
                    status = status,
                    evoucherInfo = status == "Approved" ? new {
                        code = evoucherToken.EVoucher.EVoucherCode,
                        type = evoucherToken.EVoucher.EVoucherType.TypeName,
                        value = evoucherToken.EVoucher.EVoucherType.ValueAmount
                    } : null
                });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogError(ex, "核銷處理失敗: Token={Token}, UserID={UserID}, TraceID={TraceID}", token, userId, traceId);

                return Json(new { success = false, message = "系統錯誤，請稍後再試" });
            }
        }

        /// <summary>
        /// 使用電子禮券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseEVoucher(int id)
        {
            var userId = GetCurrentUserID();
            var traceId = HttpContext.TraceIdentifier;
            var idempotencyKey = $"{userId}:{id}:use";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 60秒冪等性檢查
                var cutoffTime = DateTime.UtcNow.AddSeconds(-60);
                var duplicateCheck = await _context.EVoucherRedeemLogs
                    .AsNoTracking()
                    .AnyAsync(r => r.EVoucherID == id && 
                                 r.UserID == userId && 
                                 r.ScannedAt >= cutoffTime && 
                                 r.Status == "Approved");

                if (duplicateCheck)
                {
                    TempData["Error"] = "60秒內已有相同的使用記錄";
                    return RedirectToAction(nameof(EVouchers));
                }

                // 查詢電子禮券
                var evoucher = await _context.EVouchers
                    .Include(e => e.EVoucherType)
                    .FirstOrDefaultAsync(e => e.EVoucherID == id && e.UserID == userId);

                if (evoucher == null)
                {
                    TempData["Error"] = "找不到指定的電子禮券";
                    return RedirectToAction(nameof(EVouchers));
                }

                if (evoucher.IsUsed)
                {
                    TempData["Error"] = "此電子禮券已使用過";
                    return RedirectToAction(nameof(EVouchers));
                }

                // 檢查有效期（如果有設定）
                var now = DateTime.UtcNow;
                if (evoucher.EVoucherType.ValidTo.HasValue && evoucher.EVoucherType.ValidTo.Value < now)
                {
                    TempData["Error"] = "此電子禮券已過期";
                    return RedirectToAction(nameof(EVouchers));
                }

                // 標記為已使用
                evoucher.IsUsed = true;
                evoucher.UsedTime = now;

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "EVOUCHER_USED",
                    PointsChanged = 0, // 使用電子禮券不直接影響點數
                    ItemCode = evoucher.EVoucherCode,
                    Description = $"使用電子禮券：{evoucher.EVoucherType.TypeName}",
                    ChangeTime = now
                };

                _context.WalletHistories.Add(walletHistory);

                // 記錄兌換日誌
                var redeemLog = new EVoucherRedeemLog
                {
                    EVoucherID = id,
                    TokenID = null, // 直接使用，非 Token 掃描
                    Token = "",
                    UserID = userId,
                    ScannedAt = now,
                    Status = "Approved",
                    Location = "會員自行使用"
                };

                _context.EVoucherRedeemLogs.Add(redeemLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄 Serilog
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogInformation("會員使用電子禮券: EVoucherID={EVoucherID}, UserID={UserID}, IdempotencyKey={IdempotencyKey}, TraceID={TraceID}",
                    id, userId, idempotencyKey, traceId);

                TempData["Success"] = $"成功使用電子禮券：{evoucher.EVoucherType.TypeName}";
                return RedirectToAction(nameof(EVouchers));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogError(ex, "使用電子禮券失敗: EVoucherID={EVoucherID}, UserID={UserID}, IdempotencyKey={IdempotencyKey}, TraceID={TraceID}",
                    id, userId, idempotencyKey, traceId);

                TempData["Error"] = "使用電子禮券失敗：" + ex.Message;
                return RedirectToAction(nameof(EVouchers));
            }
        }

        /// <summary>
        /// 使用優惠券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseCoupon(int id)
        {
            var userId = GetCurrentUserID();
            var traceId = HttpContext.TraceIdentifier;
            var idempotencyKey = $"{userId}:{id}:use";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 60秒冪等性檢查
                var cutoffTime = DateTime.UtcNow.AddSeconds(-60);
                var duplicateCheck = await _context.WalletHistories
                    .AsNoTracking()
                    .AnyAsync(h => h.UserID == userId && 
                                 h.ItemCode.Contains(id.ToString()) && 
                                 h.ChangeTime >= cutoffTime && 
                                 h.ChangeType == "COUPON_USED");

                if (duplicateCheck)
                {
                    TempData["Error"] = "60秒內已有相同的使用記錄";
                    return RedirectToAction(nameof(Coupons));
                }

                // 查詢優惠券
                var coupon = await _context.Coupons
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponID == id && c.UserID == userId);

                if (coupon == null)
                {
                    TempData["Error"] = "找不到指定的優惠券";
                    return RedirectToAction(nameof(Coupons));
                }

                if (coupon.IsUsed)
                {
                    TempData["Error"] = "此優惠券已使用過";
                    return RedirectToAction(nameof(Coupons));
                }

                // 檢查有效期
                var now = DateTime.UtcNow;
                if (coupon.CouponType.ValidTo.HasValue && coupon.CouponType.ValidTo.Value < now)
                {
                    TempData["Error"] = "此優惠券已過期";
                    return RedirectToAction(nameof(Coupons));
                }

                // 標記為已使用
                coupon.IsUsed = true;
                coupon.UsedTime = now;

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "COUPON_USED",
                    PointsChanged = 0, // 使用優惠券不直接影響點數
                    ItemCode = coupon.CouponCode,
                    Description = $"使用優惠券：{coupon.CouponType.TypeName}",
                    ChangeTime = now
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄 Serilog
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogInformation("會員使用優惠券: CouponID={CouponID}, UserID={UserID}, IdempotencyKey={IdempotencyKey}, TraceID={TraceID}",
                    id, userId, idempotencyKey, traceId);

                TempData["Success"] = $"成功使用優惠券：{coupon.CouponType.TypeName}";
                return RedirectToAction(nameof(Coupons));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<WalletController>>();
                logger.LogError(ex, "使用優惠券失敗: CouponID={CouponID}, UserID={UserID}, IdempotencyKey={IdempotencyKey}, TraceID={TraceID}",
                    id, userId, idempotencyKey, traceId);

                TempData["Error"] = "使用優惠券失敗：" + ex.Message;
                return RedirectToAction(nameof(Coupons));
            }
        }

        #endregion
    }
}