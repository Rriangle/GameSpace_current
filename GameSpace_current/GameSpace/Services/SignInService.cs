using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Services
{
    public interface ISignInService
    {
        Task<bool> HasSignedInTodayAsync(int userId);
        Task<SignInResult> SignInAsync(int userId);
        Task<List<UserSignInStats>> GetUserSignInHistoryAsync(int userId, int days = 30);
        Task<int> GetConsecutiveSignInDaysAsync(int userId);
        Task<SignInReward> GetTodayRewardAsync(int userId);
    }

    public class SignInResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string? CouponGained { get; set; }
        public int ConsecutiveDays { get; set; }
    }

    public class SignInReward
    {
        public int Points { get; set; }
        public int Experience { get; set; }
        public string? Coupon { get; set; }
        public int ConsecutiveDays { get; set; }
    }

    public class SignInService : ISignInService
    {
        private readonly GameSpaceDbContext _context;
        private readonly IWalletService _walletService;

        public SignInService(GameSpaceDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<bool> HasSignedInTodayAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.UserSignInStats
                .AnyAsync(s => s.UserId == userId && s.PointsGainedTime.Date == today);
        }

        public async Task<SignInResult> SignInAsync(int userId)
        {
            // 檢查今天是否已經簽到
            if (await HasSignedInTodayAsync(userId))
            {
                return new SignInResult
                {
                    Success = false,
                    Message = "今天已經簽到過了！"
                };
            }

            // 計算連續簽到天數
            var consecutiveDays = await GetConsecutiveSignInDaysAsync(userId);
            var newConsecutiveDays = consecutiveDays + 1;

            // 計算獎勵
            var reward = CalculateSignInReward(newConsecutiveDays);

            // 創建簽到記錄
            var signInRecord = new UserSignInStats
            {
                UserId = userId,
                PointsGained = reward.Points,
                PointsGainedTime = DateTime.UtcNow,
                ExpGained = reward.Experience,
                ExpGainedTime = DateTime.UtcNow,
                CouponGained = reward.Coupon ?? "0",
                CouponGainedTime = DateTime.UtcNow
            };

            _context.UserSignInStats.Add(signInRecord);

            // 增加用戶點數
            await _walletService.AddPointsAsync(userId, reward.Points, "每日簽到獎勵");

            // 更新用戶經驗值
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.UserExp += reward.Experience;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new SignInResult
            {
                Success = true,
                Message = $"簽到成功！連續簽到 {newConsecutiveDays} 天",
                PointsGained = reward.Points,
                ExpGained = reward.Experience,
                CouponGained = reward.Coupon,
                ConsecutiveDays = newConsecutiveDays
            };
        }

        public async Task<List<UserSignInStats>> GetUserSignInHistoryAsync(int userId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            return await _context.UserSignInStats
                .Where(s => s.UserId == userId && s.PointsGainedTime >= startDate)
                .OrderByDescending(s => s.PointsGainedTime)
                .ToListAsync();
        }

        public async Task<int> GetConsecutiveSignInDaysAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var consecutiveDays = 0;

            // 從今天開始往前檢查連續簽到天數
            for (int i = 0; i < 365; i++) // 最多檢查一年
            {
                var checkDate = today.AddDays(-i);
                var hasSignedIn = await _context.UserSignInStats
                    .AnyAsync(s => s.UserId == userId && s.PointsGainedTime.Date == checkDate);

                if (hasSignedIn)
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

        public async Task<SignInReward> GetTodayRewardAsync(int userId)
        {
            var consecutiveDays = await GetConsecutiveSignInDaysAsync(userId);
            return CalculateSignInReward(consecutiveDays + 1);
        }

        private SignInReward CalculateSignInReward(int consecutiveDays)
        {
            var basePoints = 10;
            var baseExp = 5;
            var bonusMultiplier = Math.Min(consecutiveDays / 7, 5); // 每7天增加1倍，最多5倍

            var points = basePoints + (consecutiveDays * 2) + (bonusMultiplier * 10);
            var exp = baseExp + (consecutiveDays * 1) + (bonusMultiplier * 5);

            // 特殊獎勵
            string? coupon = null;
            if (consecutiveDays % 7 == 0) // 每7天給優惠券
            {
                coupon = $"COUPON_{consecutiveDays}";
            }

            return new SignInReward
            {
                Points = points,
                Experience = exp,
                Coupon = coupon,
                ConsecutiveDays = consecutiveDays
            };
        }
    }
}