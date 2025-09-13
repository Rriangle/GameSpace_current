using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class SignInController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public SignInController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 簽到頁面
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;
            
            // 檢查今日是否已簽到
            var todaySignIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == today);

            // 取得本月簽到記錄
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthSignIns = await _context.UserSignInStats
                .Where(s => s.UserId == userId && s.SignTime.Date >= monthStart && s.SignTime.Date <= monthEnd)
                .OrderBy(s => s.SignTime)
                .ToListAsync();

            // 計算連續簽到天數
            var consecutiveDays = CalculateConsecutiveDays(monthSignIns, today);

            ViewBag.HasSignedInToday = todaySignIn != null;
            ViewBag.TodaySignIn = todaySignIn;
            ViewBag.MonthSignIns = monthSignIns;
            ViewBag.ConsecutiveDays = consecutiveDays;
            ViewBag.Today = today;

            return View();
        }

        // 執行簽到
        [HttpPost]
        public async Task<IActionResult> SignIn()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            // 檢查今日是否已簽到
            var todaySignIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == today);

            if (todaySignIn != null)
            {
                return Json(new { success = false, message = "今日已簽到" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 取得本月簽到記錄
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthSignIns = await _context.UserSignInStats
                    .Where(s => s.UserId == userId && s.SignTime.Date >= monthStart && s.SignTime.Date <= monthEnd)
                    .OrderBy(s => s.SignTime)
                    .ToListAsync();

                // 計算連續簽到天數
                var consecutiveDays = CalculateConsecutiveDays(monthSignIns, today);

                // 計算獎勵
                var rewards = CalculateSignInRewards(consecutiveDays + 1, today);

                // 創建簽到記錄
                var signInRecord = new UserSignInStats
                {
                    UserId = userId,
                    SignTime = DateTime.UtcNow,
                    PointsChanged = rewards.PointsGained,
                    PointsChangedTime = DateTime.UtcNow,
                    ExpGained = rewards.ExpGained,
                    ExpGainedTime = DateTime.UtcNow,
                    CouponGained = rewards.CouponGained,
                    CouponGainedTime = DateTime.UtcNow
                };

                _context.UserSignInStats.Add(signInRecord);

                // 更新用戶點數
                if (rewards.PointsGained > 0)
                {
                    var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                    if (userWallet != null)
                    {
                        userWallet.UserPoint += rewards.PointsGained;

                        // 記錄錢包歷史
                        _context.WalletHistories.Add(new WalletHistory
                        {
                            UserId = userId,
                            ChangeType = "Point",
                            PointsChanged = rewards.PointsGained,
                            Description = "每日簽到獎勵",
                            ChangeTime = DateTime.UtcNow
                        });
                    }
                }

                // 更新寵物經驗值
                if (rewards.ExpGained > 0)
                {
                    var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
                    if (pet != null)
                    {
                        pet.Experience += rewards.ExpGained;

                        // 檢查升級
                        var requiredExp = CalculateRequiredExp(pet.Level);
                        if (pet.Experience >= requiredExp)
                        {
                            pet.Level++;
                            pet.Experience -= requiredExp;
                            pet.LevelUpTime = DateTime.UtcNow;

                            var levelUpReward = CalculateLevelUpReward(pet.Level);
                            pet.PointsGainedLevelUp = levelUpReward;
                            pet.PointsGainedTimeLevelUp = DateTime.UtcNow;

                            // 增加用戶點數
                            var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                            if (userWallet != null)
                            {
                                userWallet.UserPoint += levelUpReward;

                                // 記錄錢包歷史
                                _context.WalletHistories.Add(new WalletHistory
                                {
                                    UserId = userId,
                                    ChangeType = "Point",
                                    PointsChanged = levelUpReward,
                                    Description = "寵物升級獎勵",
                                    ChangeTime = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                // 發放優惠券
                if (rewards.CouponGained > 0)
                {
                    var couponType = await _context.CouponTypes.FirstOrDefaultAsync();
                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserId = userId,
                            CouponTypeId = couponType.CouponTypeId,
                            CouponCode = GenerateCouponCode(),
                            IsUsed = false,
                            AcquiredTime = DateTime.UtcNow
                        };
                        _context.Coupons.Add(coupon);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = GetSignInMessage(consecutiveDays + 1, rewards),
                    pointsGained = rewards.PointsGained,
                    expGained = rewards.ExpGained,
                    couponGained = rewards.CouponGained,
                    consecutiveDays = consecutiveDays + 1
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "簽到失敗" });
            }
        }

        // 簽到歷史
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            var signIns = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .Take(30)
                .ToListAsync();

            return View(signIns);
        }

        private int CalculateConsecutiveDays(List<UserSignInStats> monthSignIns, DateTime today)
        {
            if (monthSignIns.Count == 0) return 0;

            var consecutiveDays = 0;
            var checkDate = today.AddDays(-1);

            for (int i = monthSignIns.Count - 1; i >= 0; i--)
            {
                if (monthSignIns[i].SignTime.Date == checkDate)
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

        private (int PointsGained, int ExpGained, int CouponGained) CalculateSignInRewards(int consecutiveDays, DateTime today)
        {
            var pointsGained = 20; // 基本點數
            var expGained = 0;
            var couponGained = 0;

            // 假日獎勵
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
            {
                pointsGained = 30;
                expGained = 200;
            }

            // 連續簽到獎勵
            if (consecutiveDays >= 7)
            {
                pointsGained += 40;
                expGained += 300;
                couponGained = 1;
            }

            if (consecutiveDays >= 14)
            {
                pointsGained += 60;
                expGained += 500;
            }

            if (consecutiveDays >= 30)
            {
                pointsGained += 200;
                expGained += 2000;
                couponGained = 2;
            }

            return (pointsGained, expGained, couponGained);
        }

        private string GetSignInMessage(int consecutiveDays, (int PointsGained, int ExpGained, int CouponGained) rewards)
        {
            var message = $"簽到成功！獲得 {rewards.PointsGained} 點數";
            
            if (rewards.ExpGained > 0)
            {
                message += $"、{rewards.ExpGained} 經驗值";
            }
            
            if (rewards.CouponGained > 0)
            {
                message += $"、{rewards.CouponGained} 張優惠券";
            }

            if (consecutiveDays > 1)
            {
                message += $"（連續簽到 {consecutiveDays} 天）";
            }

            return message;
        }

        private int CalculateRequiredExp(int level)
        {
            if (level <= 10)
                return 40 * level + 60;
            else if (level <= 100)
                return (int)(0.8 * level * level + 380);
            else
                return (int)(285.69 * Math.Pow(1.06, level));
        }

        private int CalculateLevelUpReward(int level)
        {
            if (level <= 10)
                return level * 10;
            else if (level <= 20)
                return level * 20;
            else if (level <= 30)
                return level * 30;
            else if (level <= 40)
                return level * 40;
            else if (level <= 50)
                return level * 50;
            else
                return Math.Min(level * 60, 3000);
        }

        private string GenerateCouponCode()
        {
            return $"COUPON{Random.Shared.Next(100000, 999999)}";
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}