using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class TestController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public TestController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                Users = await _context.Users.CountAsync(),
                Pets = await _context.Pets.CountAsync(),
                UserWallets = await _context.UserWallets.CountAsync(),
                UserSignInStats = await _context.UserSignInStats.CountAsync(),
                MiniGames = await _context.MiniGames.CountAsync(),
                WalletHistory = await _context.WalletHistory.CountAsync(),
                CouponTypes = await _context.CouponTypes.CountAsync(),
                EVoucherTypes = await _context.EvoucherTypes.CountAsync(),
                Coupons = await _context.Coupons.CountAsync(),
                Evouchers = await _context.Evouchers.CountAsync(),
                EvoucherTokens = await _context.EvoucherTokens.CountAsync(),
                EvoucherRedeemLogs = await _context.EvoucherRedeemLogs.CountAsync(),
                UserIntroduces = await _context.UserIntroduces.CountAsync(),
                UserRights = await _context.UserRights.CountAsync(),
                UserTokens = await _context.UserTokens.CountAsync(),
                UserSalesInformations = await _context.UserSalesInformations.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunSeed()
        {
            try
            {
                var seedingService = new DataSeedingService(_context);
                await seedingService.SeedAllDataAsync();
                
                TempData["Message"] = "資料庫種子服務執行成功！";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"種子服務執行失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TestSignInFlow()
        {
            try
            {
                // 取得第一個用戶進行測試
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    TempData["Message"] = "沒有找到測試用戶";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                // 模擬簽到流程
                var signInController = new UserSignInStatsController(_context);
                
                // 檢查連續簽到天數
                var consecutiveDays = await CalculateConsecutiveDays(user.UserId);
                
                // 計算獎勵
                var isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
                var rewards = CalculateSignInRewards(consecutiveDays, isWeekend);

                // 建立簽到記錄
                var signInStat = new UserSignInStat
                {
                    UserId = user.UserId,
                    SignInDate = DateTime.Now,
                    ConsecutiveDays = consecutiveDays + 1,
                    PointsGained = rewards.Points,
                    ExperienceGained = rewards.Experience,
                    CouponCode = rewards.CouponCode,
                    CreatedTime = DateTime.Now
                };

                _context.UserSignInStats.Add(signInStat);

                // 更新用戶錢包
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                if (wallet != null)
                {
                    wallet.Points += rewards.Points;
                    wallet.UpdatedTime = DateTime.Now;

                    // 記錄錢包歷史
                    var walletHistory = new WalletHistory
                    {
                        UserId = user.UserId,
                        TransactionType = "簽到獎勵",
                        Amount = rewards.Points,
                        Description = $"連續簽到{consecutiveDays + 1}天獎勵",
                        CreatedTime = DateTime.Now
                    };

                    _context.WalletHistory.Add(walletHistory);
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = $"簽到測試成功！用戶 {user.UserName} 獲得 {rewards.Points} 點數，{rewards.Experience} 經驗值";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"簽到測試失敗：{ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index");
        }

        private async Task<int> CalculateConsecutiveDays(int userId)
        {
            var today = DateTime.Today;
            var consecutiveDays = 0;

            for (int i = 0; i < 30; i++) // 最多檢查30天
            {
                var checkDate = today.AddDays(-i);
                var hasSignIn = await _context.UserSignInStats
                    .AnyAsync(s => s.UserId == userId && s.SignInDate.Date == checkDate);

                if (hasSignIn)
                {
                    consecutiveDays++;
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        private (int Points, int Experience, string CouponCode) CalculateSignInRewards(int consecutiveDays, bool isWeekend)
        {
            var points = 10; // 基礎點數
            var experience = 5; // 基礎經驗值
            string couponCode = null;

            // 連續簽到獎勵
            if (consecutiveDays >= 7)
            {
                points += 50;
                experience += 25;
                couponCode = "WEEKLY_BONUS";
            }
            else if (consecutiveDays >= 3)
            {
                points += 20;
                experience += 10;
            }

            // 週末獎勵
            if (isWeekend)
            {
                points += 15;
                experience += 8;
            }

            return (points, experience, couponCode);
        }
    }
}