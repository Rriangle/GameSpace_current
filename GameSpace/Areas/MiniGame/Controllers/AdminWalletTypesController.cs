using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - 券類型管理控制器（CRUD 功能）
    /// 根據指令第[4]節，僅針對型別表提供 CRUD：CouponType, EVoucherType
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
    public class AdminWalletTypesController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletTypesController(GameSpaceDbContext context)
        {
            _context = context;
        }

        #region CouponType CRUD

        /// <summary>
        /// 優惠券類型列表
        /// </summary>
        public async Task<IActionResult> CouponTypes()
        {
            var couponTypes = await _context.CouponTypes
                .AsNoTracking()
                .OrderBy(ct => ct.Name)
                .ToListAsync();

            return View(couponTypes);
        }

        /// <summary>
        /// 新增優惠券類型頁面
        /// </summary>
        public IActionResult CreateCouponType()
        {
            return View();
        }

        /// <summary>
        /// 新增優惠券類型處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCouponType(CouponType couponType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.CouponTypes.Add(couponType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "優惠券類型新增成功";
                    return RedirectToAction(nameof(CouponTypes));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"新增失敗：{ex.Message}";
                }
            }

            return View(couponType);
        }

        /// <summary>
        /// 編輯優惠券類型頁面
        /// </summary>
        public async Task<IActionResult> EditCouponType(int id)
        {
            var couponType = await _context.CouponTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CouponTypeID == id);

            if (couponType == null)
            {
                return NotFound("找不到指定的優惠券類型");
            }

            return View(couponType);
        }

        /// <summary>
        /// 編輯優惠券類型處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCouponType(int id, CouponType couponType)
        {
            if (id != couponType.CouponTypeID)
            {
                return BadRequest("ID 不匹配");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 手動設置UpdatedAt以確保DB欄位100%覆蓋
                    couponType.UpdatedAt = DateTime.UtcNow;
                    _context.Update(couponType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "優惠券類型更新成功";
                    return RedirectToAction(nameof(CouponTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CouponTypeExistsAsync(couponType.CouponTypeID))
                    {
                        return NotFound("優惠券類型不存在");
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"更新失敗：{ex.Message}";
                }
            }

            return View(couponType);
        }

        /// <summary>
        /// 刪除優惠券類型
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCouponType(int id)
        {
            try
            {
                var couponType = await _context.CouponTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.CouponTypeID == id);

                if (couponType == null)
                {
                    return NotFound("找不到指定的優惠券類型");
                }

                // 檢查是否有相關的優惠券使用此類型
                var hasRelatedCoupons = await _context.Coupons
                    .AsNoTracking()
                    .AnyAsync(c => c.CouponTypeID == id);

                if (hasRelatedCoupons)
                {
                    TempData["ErrorMessage"] = "此優惠券類型已有相關優惠券，無法刪除";
                    return RedirectToAction(nameof(CouponTypes));
                }

                _context.CouponTypes.Remove(couponType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "優惠券類型刪除成功";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(CouponTypes));
        }

        #endregion

        #region EVoucherType CRUD

        /// <summary>
        /// 電子禮券類型列表
        /// </summary>
        public async Task<IActionResult> EVoucherTypes()
        {
            var evoucherTypes = await _context.EVoucherTypes
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();

            return View(evoucherTypes);
        }

        /// <summary>
        /// 新增電子禮券類型頁面
        /// </summary>
        public IActionResult CreateEVoucherType()
        {
            return View();
        }

        /// <summary>
        /// 新增電子禮券類型處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEVoucherType(EVoucherType evoucherType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.EVoucherTypes.Add(evoucherType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "電子禮券類型新增成功";
                    return RedirectToAction(nameof(EVoucherTypes));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"新增失敗：{ex.Message}";
                }
            }

            return View(evoucherType);
        }

        /// <summary>
        /// 編輯電子禮券類型頁面
        /// </summary>
        public async Task<IActionResult> EditEVoucherType(int id)
        {
            var evoucherType = await _context.EVoucherTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(et => et.EVoucherTypeID == id);

            if (evoucherType == null)
            {
                return NotFound("找不到指定的電子禮券類型");
            }

            return View(evoucherType);
        }

        /// <summary>
        /// 編輯電子禮券類型處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEVoucherType(int id, EVoucherType evoucherType)
        {
            if (id != evoucherType.EVoucherTypeID)
            {
                return BadRequest("ID 不匹配");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 手動設置UpdatedAt以確保DB欄位100%覆蓋
                    evoucherType.UpdatedAt = DateTime.UtcNow;
                    _context.Update(evoucherType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "電子禮券類型更新成功";
                    return RedirectToAction(nameof(EVoucherTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EVoucherTypeExistsAsync(evoucherType.EVoucherTypeID))
                    {
                        return NotFound("電子禮券類型不存在");
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"更新失敗：{ex.Message}";
                }
            }

            return View(evoucherType);
        }

        /// <summary>
        /// 刪除電子禮券類型
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEVoucherType(int id)
        {
            try
            {
                var evoucherType = await _context.EVoucherTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(et => et.EVoucherTypeID == id);

                if (evoucherType == null)
                {
                    return NotFound("找不到指定的電子禮券類型");
                }

                // 檢查是否有相關的電子禮券使用此類型
                var hasRelatedEVouchers = await _context.EVouchers
                    .AsNoTracking()
                    .AnyAsync(e => e.EVoucherTypeID == id);

                if (hasRelatedEVouchers)
                {
                    TempData["ErrorMessage"] = "此電子禮券類型已有相關禮券，無法刪除";
                    return RedirectToAction(nameof(EVoucherTypes));
                }

                _context.EVoucherTypes.Remove(evoucherType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "電子禮券類型刪除成功";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(EVoucherTypes));
        }

        #endregion

        #region Helper Methods

        private async Task<bool> CouponTypeExistsAsync(int id)
        {
            return await _context.CouponTypes.AsNoTracking().AnyAsync(e => e.CouponTypeID == id);
        }

        private async Task<bool> EVoucherTypeExistsAsync(int id)
        {
            return await _context.EVoucherTypes.AsNoTracking().AnyAsync(e => e.EVoucherTypeID == id);
        }

        #endregion
    }
}