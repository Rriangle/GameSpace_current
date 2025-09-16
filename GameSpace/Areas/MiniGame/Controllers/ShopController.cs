using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class ShopController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public ShopController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 商城首頁
        public async Task<IActionResult> Index()
        {
            var products = await _context.ProductInfos
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Price)
                .ToListAsync();

            var categories = await _context.ProductInfos
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            return View(products);
        }

        // 商品詳情
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.ProductInfos
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 購買商品
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int productId, int quantity = 1)
        {
            var userId = GetCurrentUserID();
            var product = await _context.ProductInfos
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

            if (product == null)
            {
                return Json(new { success = false, message = "商品不存在或已下架" });
            }

            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (userWallet == null)
            {
                return Json(new { success = false, message = "找不到錢包" });
            }

            var totalCost = product.Price * quantity;

            if (userWallet.UserPoint < totalCost)
            {
                return Json(new { success = false, message = "點數不足" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除點數
                userWallet.UserPoint -= totalCost;

                // 創建訂單
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalCost,
                    Status = "已付款"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 創建訂單詳情
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    TotalPrice = totalCost
                };

                _context.OrderDetails.Add(orderDetail);

                // 記錄錢包歷史
                _context.WalletHistories.Add(new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = -totalCost,
                    ItemCode = order.OrderId.ToString(),
                    Description = $"購買商品：{product.ProductName} x{quantity}",
                    ChangeTime = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = $"購買成功！訂單編號：{order.OrderId}",
                    orderId = order.OrderId,
                    points = userWallet.UserPoint
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "購買失敗" });
            }
        }

        // 我的訂單
        public async Task<IActionResult> Orders()
        {
            var userId = GetCurrentUserID();
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductInfo)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 訂單詳情
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetCurrentUserID();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductInfo)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // 搜尋商品
        public async Task<IActionResult> Search(string keyword, string category)
        {
            var query = _context.ProductInfos.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductName.Contains(keyword) || 
                                       p.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Price)
                .ToListAsync();

            var categories = await _context.ProductInfos
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Keyword = keyword;
            ViewBag.SelectedCategory = category;

            return View("Index", products);
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
    }
}