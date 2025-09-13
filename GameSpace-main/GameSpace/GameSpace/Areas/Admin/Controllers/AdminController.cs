using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public AdminController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPets = await _context.Pets.CountAsync(),
                TotalMiniGames = await _context.MiniGames.CountAsync(),
                TotalCoupons = await _context.Coupons.CountAsync(),
                TotalEvouchers = await _context.Evouchers.CountAsync(),
                TotalWalletHistory = await _context.WalletHistory.CountAsync(),
                TotalSignIns = await _context.UserSignInStats.CountAsync(),
                TotalUserIntroduces = await _context.UserIntroduces.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.UserWallets)
                .Include(u => u.Pets)
                .Include(u => u.UserSignInStats)
                .OrderByDescending(u => u.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Pets()
        {
            var pets = await _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(pets);
        }

        public async Task<IActionResult> MiniGames()
        {
            var miniGames = await _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .OrderByDescending(m => m.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(miniGames);
        }

        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .OrderByDescending(c => c.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(coupons);
        }

        public async Task<IActionResult> Evouchers()
        {
            var evouchers = await _context.Evouchers
                .Include(e => e.User)
                .Include(e => e.EvoucherType)
                .OrderByDescending(e => e.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(evouchers);
        }

        public async Task<IActionResult> WalletHistory()
        {
            var walletHistory = await _context.WalletHistory
                .Include(w => w.User)
                .OrderByDescending(w => w.CreatedTime)
                .Take(100)
                .ToListAsync();

            return View(walletHistory);
        }

        public async Task<IActionResult> UserIntroduces()
        {
            var userIntroduces = await _context.UserIntroduces
                .Include(u => u.User)
                .OrderByDescending(u => u.CreatedTime)
                .Take(50)
                .ToListAsync();

            return View(userIntroduces);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUserIntroduce(int id)
        {
            try
            {
                var userIntroduce = await _context.UserIntroduces.FindAsync(id);
                if (userIntroduce != null)
                {
                    userIntroduce.IsApproved = true;
                    userIntroduce.ApprovedTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    TempData["Message"] = "用戶介紹已審核通過";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "找不到指定的用戶介紹";
                    TempData["MessageType"] = "warning";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"審核失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("UserIntroduces");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUserIntroduce(int id)
        {
            try
            {
                var userIntroduce = await _context.UserIntroduces.FindAsync(id);
                if (userIntroduce != null)
                {
                    userIntroduce.IsApproved = false;
                    userIntroduce.ApprovedTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    TempData["Message"] = "用戶介紹已審核拒絕";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "找不到指定的用戶介紹";
                    TempData["MessageType"] = "warning";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"審核失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("UserIntroduces");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    // 刪除相關資料
                    var pets = await _context.Pets.Where(p => p.UserId == id).ToListAsync();
                    var wallets = await _context.UserWallets.Where(w => w.UserId == id).ToListAsync();
                    var signIns = await _context.UserSignInStats.Where(s => s.UserId == id).ToListAsync();
                    var miniGames = await _context.MiniGames.Where(m => m.UserId == id).ToListAsync();
                    var coupons = await _context.Coupons.Where(c => c.UserId == id).ToListAsync();
                    var evouchers = await _context.Evouchers.Where(e => e.UserId == id).ToListAsync();
                    var walletHistory = await _context.WalletHistory.Where(w => w.UserId == id).ToListAsync();
                    var userIntroduces = await _context.UserIntroduces.Where(u => u.UserId == id).ToListAsync();

                    _context.Pets.RemoveRange(pets);
                    _context.UserWallets.RemoveRange(wallets);
                    _context.UserSignInStats.RemoveRange(signIns);
                    _context.MiniGames.RemoveRange(miniGames);
                    _context.Coupons.RemoveRange(coupons);
                    _context.Evouchers.RemoveRange(evouchers);
                    _context.WalletHistory.RemoveRange(walletHistory);
                    _context.UserIntroduces.RemoveRange(userIntroduces);
                    _context.Users.Remove(user);

                    await _context.SaveChangesAsync();
                    
                    TempData["Message"] = "用戶及其相關資料已刪除";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "找不到指定的用戶";
                    TempData["MessageType"] = "warning";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"刪除失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePet(int id)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(id);
                if (pet != null)
                {
                    _context.Pets.Remove(pet);
                    await _context.SaveChangesAsync();
                    
                    TempData["Message"] = "寵物已刪除";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "找不到指定的寵物";
                    TempData["MessageType"] = "warning";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"刪除失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Pets");
        }
    }
}