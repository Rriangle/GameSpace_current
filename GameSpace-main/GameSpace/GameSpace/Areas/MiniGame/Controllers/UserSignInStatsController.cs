using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class UserSignInStatsController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public UserSignInStatsController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/UserSignInStats
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            
            // 取得用戶簽到記錄
            var signInStats = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .Take(30) // 只顯示最近30筆記錄
                .ToListAsync();

            // 計算連續簽到天數
            var consecutiveDays = await CalculateConsecutiveDays(userId);
            ViewBag.ConsecutiveDays = consecutiveDays;

            // 檢查今日是否已簽到
            var today = DateTime.UtcNow.Date;
            var todaySignIn = signInStats.FirstOrDefault(s => s.SignTime.Date == today);
            ViewBag.HasSignedInToday = todaySignIn != null;
            ViewBag.TodaySignIn = todaySignIn;

            return View(signInStats);
        }

        // GET: MiniGame/UserSignInStats/SignIn
        public async Task<IActionResult> SignIn()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            // 檢查今日是否已簽到
            var existingSignIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == today);

            if (existingSignIn != null)
            {
                TempData["Warning"] = "今日已經簽到過了！";
                return RedirectToAction(nameof(Index));
            }

            // 計算連續簽到天數
            var consecutiveDays = await CalculateConsecutiveDays(userId);
            ViewBag.ConsecutiveDays = consecutiveDays;

            return View();
        }

        // POST: MiniGame/UserSignInStats/PerformSignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PerformSignIn()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查今日是否已簽到
                var existingSignIn = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SignTime.Date == today);

                if (existingSignIn != null)
                {
                    TempData["Warning"] = "今日已經簽到過了！";
                    return RedirectToAction(nameof(Index));
                }

                // 計算連續簽到天數
                var consecutiveDays = await CalculateConsecutiveDays(userId);
                
                // 確定是否為假日（簡化處理，週六日視為假日）
                var isWeekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;
                
                // 計算獎勵
                var (points, exp, couponCode) = CalculateSignInRewards(consecutiveDays + 1, isWeekend);
                
                // 建立簽到記錄
                var signInRecord = new UserSignInStat
                {
                    UserId = userId,
                    SignTime = DateTime.UtcNow,
                    PointsGained = points,
                    PointsGainedTime = DateTime.UtcNow,
                    ExpGained = exp,
                    ExpGainedTime = DateTime.UtcNow,
                    CouponGained = couponCode,
                    CouponGainedTime = DateTime.UtcNow
                };
                _context.UserSignInStats.Add(signInRecord);

                // 更新用戶錢包點數
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = userId, UserPoint = 0 };
                    _context.UserWallets.Add(wallet);
                }
                wallet.UserPoint += points;

                // 記錄錢包異動
                var walletHistory = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "SignIn",
                    PointsChanged = points,
                    Description = $"每日簽到獲得（連續{consecutiveDays + 1}天）",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(walletHistory);

                // 如果有寵物，增加經驗值
                if (exp > 0)
                {
                    var pet = await _context.Pets
                        .FirstOrDefaultAsync(p => p.UserId == userId);
                    if (pet != null)
                    {
                        pet.Experience += exp;
                        await CheckPetLevelUp(pet, userId);
                    }
                }

                // 如果有優惠券獎勵，建立優惠券
                if (!string.IsNullOrEmpty(couponCode) && couponCode != "0")
                {
                    await CreateSignInCoupon(userId, couponCode);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var message = $"簽到成功！獲得 {points} 點數";
                if (exp > 0) message += $"、{exp} 寵物經驗值";
                if (!string.IsNullOrEmpty(couponCode) && couponCode != "0") message += $"、優惠券：{couponCode}";
                
                TempData["Success"] = message;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"簽到失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // 計算連續簽到天數
        private async Task<int> CalculateConsecutiveDays(int userId)
        {
            var signInRecords = await _context.UserSignInStats
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SignTime)
                .Select(s => s.SignTime.Date)
                .ToListAsync();

            if (!signInRecords.Any()) return 0;

            var consecutiveDays = 0;
            var currentDate = DateTime.UtcNow.Date;
            
            // 如果今天已經簽到，從今天開始計算
            if (signInRecords.Contains(currentDate))
            {
                consecutiveDays = 1;
                currentDate = currentDate.AddDays(-1);
            }
            else
            {
                // 如果今天沒簽到，從昨天開始計算
                currentDate = currentDate.AddDays(-1);
            }

            // 往前計算連續天數
            foreach (var signInDate in signInRecords.Skip(signInRecords.Contains(DateTime.UtcNow.Date) ? 1 : 0))
            {
                if (signInDate == currentDate)
                {
                    consecutiveDays++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        // 計算簽到獎勵
        private (int points, int exp, string couponCode) CalculateSignInRewards(int consecutiveDays, bool isWeekend)
        {
            int basePoints = isWeekend ? 30 : 20; // 假日額外獎勵
            int baseExp = isWeekend ? 200 : 0;    // 假日給予寵物經驗
            string couponCode = "0";

            // 連續簽到獎勵
            if (consecutiveDays == 7)
            {
                basePoints += 40;
                baseExp += 300;
                couponCode = "STREAK7";  // 連續7天獎勵券
            }
            else if (consecutiveDays == 14)
            {
                basePoints += 80;
                baseExp += 500;
                couponCode = "STREAK14"; // 連續14天獎勵券
            }
            else if (consecutiveDays >= 30)
            {
                basePoints += 200;
                baseExp += 2000;
                couponCode = "MONTHLY"; // 全勤獎勵券
            }

            return (basePoints, baseExp, couponCode);
        }

        // 建立簽到獎勵優惠券
        private async Task CreateSignInCoupon(int userId, string couponType)
        {
            // 根據優惠券類型找到對應的CouponType
            var couponTypeRecord = await _context.CouponTypes
                .FirstOrDefaultAsync(ct => ct.Name.Contains(couponType.Contains("STREAK") ? "簽到獎勵" : "全勤獎勵"));

            if (couponTypeRecord != null)
            {
                var couponCode = GenerateCouponCode();
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeRecord.CouponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };
                _context.Coupons.Add(coupon);

                // 記錄錢包異動（優惠券獲得）
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"簽到獎勵優惠券：{couponTypeRecord.Name}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);
            }
        }

        // 檢查寵物升級（與PetController中的邏輯一致）
        private async Task CheckPetLevelUp(Pet pet, int userId)
        {
            int requiredExp = CalculateRequiredExp(pet.Level);
            
            if (pet.Experience >= requiredExp)
            {
                pet.Level++;
                pet.Experience -= requiredExp;
                pet.LevelUpTime = DateTime.UtcNow;
                
                int pointsReward = Math.Min(250, (pet.Level / 10 + 1) * 10);
                pet.PointsGainedLevelUp = pointsReward;
                pet.PointsGainedTimeLevelUp = DateTime.UtcNow;
                
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.UserPoint += pointsReward;
                }

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "PetLevelUp",
                    PointsChanged = pointsReward,
                    Description = $"寵物升級獎勵（Lv.{pet.Level}）",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);
            }
        }

        private int CalculateRequiredExp(int level)
        {
            if (level <= 10)
            {
                return 40 * level + 60;
            }
            else if (level <= 100)
            {
                return (int)(0.8 * level * level + 380);
            }
            else
            {
                return (int)(285.69 * Math.Pow(1.06, level));
            }
        }

        private string GenerateCouponCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int GetCurrentUserId()
        {
            // 暫時使用固定用戶ID進行測試
            // TODO: 實作適當的用戶身份驗證
            return 1;
        }
    }
}