using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 管理員首頁
        public async Task<IActionResult> Index()
        {
            // 統計數據
            var totalUsers = await _context.Users.CountAsync();
            var totalPets = await _context.Pets.CountAsync();
            var totalForums = await _context.Forums.CountAsync();
            var totalOrders = await _context.OrderInfos.CountAsync();
            var totalProducts = await _context.ProductInfos.CountAsync();

            // 最近活動
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentForums = await _context.Forums
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalPets = totalPets;
            ViewBag.TotalForums = totalForums;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.RecentUsers = recentUsers;
            ViewBag.RecentForums = recentForums;

            return View();
        }

        // 用戶管理
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Pets)
                .Include(u => u.UserWallet)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // 用戶詳情
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Pets)
                .Include(u => u.UserWallet)
                .Include(u => u.UserIntroduce)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // 寵物管理
        public async Task<IActionResult> Pets()
        {
            var pets = await _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .ToListAsync();

            return View(pets);
        }

        // 論壇管理
        public async Task<IActionResult> Forums()
        {
            var forums = await _context.Forums
                .Include(f => f.Threads)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(forums);
        }

        // 論壇詳情
        public async Task<IActionResult> ForumDetails(int id)
        {
            var forum = await _context.Forums
                .Include(f => f.Threads)
                .FirstOrDefaultAsync(f => f.ForumId == id);

            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // 刪除論壇
        [HttpPost]
        public async Task<IActionResult> DeleteForum(int id)
        {
            var forum = await _context.Forums.FindAsync(id);
            if (forum != null)
            {
                _context.Forums.Remove(forum);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Forums));
        }

        // 訂單管理
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.OrderInfos
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(od => od.ProductInfo)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 訂單詳情
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.OrderInfos
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(od => od.ProductInfo)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // 商品管理
        public async Task<IActionResult> Products()
        {
            var products = await _context.ProductInfos
                .OrderBy(p => p.Category)
                .ThenBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        // 商品詳情
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.ProductInfos.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 編輯商品
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.ProductInfos.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // 編輯商品 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductInfo product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Products));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(product);
        }

        // 創建商品
        public IActionResult CreateProduct()
        {
            return View();
        }

        // 創建商品 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductInfo product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.IsActive = true;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }

            return View(product);
        }

        // 刪除商品
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ProductInfos.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Products));
        }

        // 優惠券管理
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            return View(coupons);
        }

        // 禮券管理
        public async Task<IActionResult> EVouchers()
        {
            var evouchers = await _context.EVouchers
                .Include(e => e.User)
                .Include(e => e.EVoucherType)
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            return View(evouchers);
        }

        // 聊天訊息管理
        public async Task<IActionResult> ChatMessages()
        {
            var messages = await _context.ChatMessages
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .OrderByDescending(c => c.SentAt)
                .Take(100)
                .ToListAsync();

            return View(messages);
        }

        // 刪除聊天訊息
        [HttpPost]
        public async Task<IActionResult> DeleteChatMessage(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message != null)
            {
                _context.ChatMessages.Remove(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ChatMessages));
        }

        // 系統設定
        public IActionResult Settings()
        {
            return View();
        }

        private bool ProductExists(int id)
        {
            return _context.ProductInfos.Any(e => e.ProductId == id);
        }
    }
}