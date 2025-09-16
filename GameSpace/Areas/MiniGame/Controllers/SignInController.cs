using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Security.Claims;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class SignInController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public SignInController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/SignIn
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserID();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // 獲取用戶簽到統計
            var signInStats = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            // 獲取用戶寵物
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // 獲取用戶錢包
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            ViewBag.SignInStats = signInStats;
            ViewBag.Pet = pet;
            ViewBag.Wallet = wallet;
            ViewBag.UserId = userId;

            return View(signInStats);
        }

        // POST: MiniGame/SignIn/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn()
        {
            var userId = GetCurrentUserID();
            if (userId == null)
            {
                return Json(new { success = false, message = "請先登入" });
            }

            try
            {
                var today = DateTime.Today;
                
                // 檢查今天是否已經簽到
                var todaySignIn = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == today);

                if (todaySignIn != null)
                {
                    return Json(new { success = false, message = "今天已經簽到過了！" });
                }

                // 獲取用戶錢包
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return Json(new { success = false, message = "找不到用戶錢包" });
                }

                // 獲取用戶寵物
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                // 計算連續簽到天數
                var consecutiveDays = await CalculateConsecutiveDays(userId, today);

                // 計算獎勵
                var rewards = CalculateRewards(consecutiveDays + 1, today);

                // 創建簽到記錄
                var signInRecord = new UserSignInStats
                {
                    UserId = userId.Value,
                    SignTime = DateTime.Now,
                    ConsecutiveDays = consecutiveDays + 1,
                    PointsEarned = rewards.Points,
                    ExperienceEarned = rewards.Experience,
                    CouponEarned = rewards.Coupon
                };

                _context.UserSignInStats.Add(signInRecord);

                // 更新錢包點數
                wallet.UserPoint += rewards.Points;

                // 更新寵物經驗
                if (pet != null)
                {
                    pet.Experience += rewards.Experience;
                    
                    // 檢查是否升級
                    var newLevel = CalculateLevel(pet.Experience);
                    if (newLevel > pet.Level)
                    {
                        pet.Level = newLevel;
                        pet.SkinColorChangedTime = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"簽到成功！獲得 {rewards.Points} 點數" + 
                             (rewards.Experience > 0 ? $" 和 {rewards.Experience} 經驗" : "") +
                             (rewards.Coupon ? " 和優惠券" : ""),
                    consecutiveDays = consecutiveDays + 1,
                    pointsEarned = rewards.Points,
                    experienceEarned = rewards.Experience,
                    couponEarned = rewards.Coupon
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "簽到失敗：" + ex.Message });
            }
        }

        /// <summary>
        /// 取得當前登入會員 ID
        /// </summary>
        private int? GetCurrentUserID()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userID))
                {
                    return userID;
                }
            }
            return null;
        }

        private async Task<int> CalculateConsecutiveDays(int userId, DateTime today)
        {
            var consecutiveDays = 0;
            var checkDate = today.AddDays(-1);

            while (true)
            {
                var signInRecord = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == checkDate);

                if (signInRecord != null)
                {
                    consecutiveDays++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        private (int Points, int Experience, bool Coupon) CalculateRewards(int consecutiveDays, DateTime today)
        {
            var points = 20; // 基礎點數
            var experience = 0;
            var coupon = false;

            // 假日獎勵
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
            {
                points += 10;
                experience += 200;
            }

            // 連續簽到獎勵
            if (consecutiveDays >= 7)
            {
                points += 40;
                experience += 300;
                coupon = true;
            }

            if (consecutiveDays >= 30)
            {
                points += 200;
                experience += 2000;
                coupon = true;
            }

            return (points, experience, coupon);
        }

        private int CalculateLevel(int experience)
        {
            if (experience <= 100)
            {
                return (experience - 60) / 40 + 1;
            }
            else if (experience <= 10000)
            {
                return (int)Math.Sqrt((experience - 380) / 0.8) + 1;
            }
            else
            {
                return (int)(Math.Log(experience / 285.69) / Math.Log(1.06)) + 1;
            }
        }
    }
}