using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class FakeDataService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly Random _random;

        public FakeDataService(GameSpacedatabaseContext context)
        {
            _context = context;
            _random = new Random(42); // Fixed seed for reproducible data
        }

        public async Task GenerateFakeDataAsync()
        {
            await GenerateUsersAsync();
            await GeneratePetsAsync();
            await GenerateUserWalletsAsync();
            await GenerateSignInStatsAsync();
            await GenerateMiniGamesAsync();
            await GenerateCouponsAsync();
            await GenerateEVouchersAsync();
        }

        private async Task GenerateUsersAsync()
        {
            var existingCount = await _context.Users.CountAsync();
            if (existingCount >= 200) return;

            var users = new List<User>();
            for (int i = existingCount; i < 200; i++)
            {
                users.Add(new User
                {
                    UserId = i + 1,
                    UserName = $"user{i + 1:D3}",
                    UserAccount = $"user{i + 1:D3}@gamespace.com",
                    UserPassword = "AQAAAAEAACcQAAAAEHashPassword", // Placeholder hash
                    UserEmailConfirmed = true,
                    UserPhoneConfirmed = true,
                    UserAccessFailedCount = 0,
                    UserLockoutEnabled = false,
                    UserLockoutEnd = null
                });
            }

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task GeneratePetsAsync()
        {
            var existingCount = await _context.Pets.CountAsync();
            if (existingCount >= 200) return;

            var pets = new List<Pet>();
            for (int i = existingCount; i < 200; i++)
            {
                pets.Add(new Pet
                {
                    PetId = i + 1,
                    UserId = i + 1,
                    PetName = $"Pet{i + 1}",
                    Level = _random.Next(1, 10),
                    LevelUpTime = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                    Experience = _random.Next(0, 1000),
                    Hunger = _random.Next(0, 100),
                    Mood = _random.Next(0, 100),
                    Stamina = _random.Next(0, 100),
                    Cleanliness = _random.Next(0, 100),
                    Health = _random.Next(50, 100),
                    SkinColor = $"#{_random.Next(0x1000000):X6}",
                    ColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                    BackgroundColor = "Default",
                    BackgroundColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                    PointsChangedColor = 0,
                    PointsChangedTimeColor = DateTime.UtcNow,
                    PointsGainedLevelUp = _random.Next(10, 50),
                    PointsGainedTimeLevelUp = DateTime.UtcNow.AddDays(-_random.Next(1, 30))
                });
            }

            await _context.Pets.AddRangeAsync(pets);
            await _context.SaveChangesAsync();
        }

        private async Task GenerateUserWalletsAsync()
        {
            var existingCount = await _context.UserWallets.CountAsync();
            if (existingCount >= 200) return;

            var wallets = new List<UserWallet>();
            for (int i = existingCount; i < 200; i++)
            {
                wallets.Add(new UserWallet
                {
                    UserId = i + 1,
                    UserPoint = _random.Next(0, 10000)
                });
            }

            await _context.UserWallets.AddRangeAsync(wallets);
            await _context.SaveChangesAsync();
        }

        private async Task GenerateSignInStatsAsync()
        {
            var existingCount = await _context.UserSignInStats.CountAsync();
            if (existingCount >= 200) return;

            var signIns = new List<UserSignInStat>();
            for (int i = existingCount; i < 200; i++)
            {
                signIns.Add(new UserSignInStat
                {
                    LogId = i + 1,
                    SignTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    UserId = _random.Next(1, 200),
                    PointsChanged = _random.Next(10, 50),
                    ExpGained = _random.Next(5, 25),
                    CouponGained = _random.Next(0, 2) == 1 ? $"COUPON{i + 1}" : "0",
                    PointsChangedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    ExpGainedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    CouponGainedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                });
            }

            await _context.UserSignInStats.AddRangeAsync(signIns);
            await _context.SaveChangesAsync();
        }

        private async Task GenerateMiniGamesAsync()
        {
            var existingCount = await _context.MiniGames.CountAsync();
            if (existingCount >= 200) return;

            var games = new List<MiniGame>();
            for (int i = existingCount; i < 200; i++)
            {
                games.Add(new MiniGame
                {
                    PlayId = i + 1,
                    UserId = _random.Next(1, 200),
                    PetId = _random.Next(1, 200),
                    Level = _random.Next(1, 3),
                    MonsterCount = _random.Next(5, 15),
                    SpeedMultiplier = (decimal)(_random.NextDouble() * 2 + 0.5),
                    Result = _random.Next(0, 3) switch { 0 => "Win", 1 => "Lose", _ => "Abort" },
                    ExpGained = _random.Next(0, 100),
                    ExpGainedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    PointsChanged = _random.Next(0, 50),
                    PointsChangedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    CouponGained = _random.Next(0, 2) == 1 ? $"GAME_COUPON{i + 1}" : "0",
                    CouponGainedTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    HungerDelta = _random.Next(-20, 5),
                    MoodDelta = _random.Next(-10, 15),
                    StaminaDelta = _random.Next(-30, 5),
                    CleanlinessDelta = _random.Next(-15, 5),
                    StartTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    EndTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)).AddMinutes(_random.Next(5, 60)),
                    Aborted = _random.Next(0, 10) == 0
                });
            }

            await _context.MiniGames.AddRangeAsync(games);
            await _context.SaveChangesAsync();
        }

        private async Task GenerateCouponsAsync()
        {
            var existingCount = await _context.Coupons.CountAsync();
            if (existingCount >= 200) return;

            var coupons = new List<Coupon>();
            for (int i = existingCount; i < 200; i++)
            {
                coupons.Add(new Coupon
                {
                    CouponId = i + 1,
                    CouponCode = $"COUPON{i + 1:D6}",
                    CouponTypeId = _random.Next(1, 5),
                    UserId = _random.Next(1, 200),
                    IsUsed = _random.Next(0, 3) == 0,
                    AcquiredTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    UsedTime = _random.Next(0, 3) == 0 ? DateTime.UtcNow.AddDays(-_random.Next(0, 15)) : null,
                    UsedInOrderId = _random.Next(1, 100)
                });
            }

            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();
        }

        private async Task GenerateEVouchersAsync()
        {
            var existingCount = await _context.Evouchers.CountAsync();
            if (existingCount >= 200) return;

            var evouchers = new List<Evoucher>();
            for (int i = existingCount; i < 200; i++)
            {
                evouchers.Add(new Evoucher
                {
                    EvoucherId = i + 1,
                    EvoucherCode = $"EVOUCHER{i + 1:D8}",
                    EvoucherTypeId = _random.Next(1, 3),
                    UserId = _random.Next(1, 200),
                    IsUsed = _random.Next(0, 4) == 0,
                    AcquiredTime = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                    UsedTime = _random.Next(0, 4) == 0 ? DateTime.UtcNow.AddDays(-_random.Next(0, 15)) : null
                });
            }

            await _context.Evouchers.AddRangeAsync(evouchers);
            await _context.SaveChangesAsync();
        }
    }
}