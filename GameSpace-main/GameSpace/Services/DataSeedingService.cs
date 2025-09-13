using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Services
{
    public class DataSeedingService
    {
        private readonly GameSpaceDbContext _context;
        private readonly Random _random = new Random();

        public DataSeedingService(GameSpaceDbContext context)
        {
            _context = context;
        }

        public async Task SeedAllDataAsync()
        {
            await SeedUsersAsync();
            await SeedPetsAsync();
            await SeedCouponTypesAsync();
            await SeedEVoucherTypesAsync();
            await SeedCouponsAsync();
            await SeedEVouchersAsync();
            await SeedUserSignInStatsAsync();
            await SeedMiniGamesAsync();
            await SeedWalletHistoriesAsync();
            await SeedUserIntroducesAsync();
            await SeedUserRightsAsync();
            await SeedUserWalletsAsync();
            await SeedUserTokensAsync();
            await SeedUserSalesInformationsAsync();
            await SeedEVoucherTokensAsync();
            await SeedEVoucherRedeemLogsAsync();
        }

        private async Task SeedUsersAsync()
        {
            var existingCount = await _context.Users.CountAsync();
            if (existingCount >= 200) return;

            var users = new List<User>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                users.Add(new User
                {
                    UserName = $"用戶{i}",
                    UserAccount = $"user{i:D3}",
                    UserPassword = BCrypt.Net.BCrypt.HashPassword("Password001@"),
                    UserEmail = $"user{i}@example.com",
                    UserPhone = $"09{i:D8}",
                    UserEmailConfirmed = true,
                    UserPhoneConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(365)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPetsAsync()
        {
            var existingCount = await _context.Pets.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var pets = new List<Pet>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                pets.Add(new Pet
                {
                    UserId = user.UserId,
                    PetName = $"寵物{i}",
                    Level = _random.Next(1, 50),
                    Experience = _random.Next(0, 1000),
                    Hunger = _random.Next(0, 101),
                    Mood = _random.Next(0, 101),
                    Stamina = _random.Next(0, 101),
                    Cleanliness = _random.Next(0, 101),
                    Health = _random.Next(0, 101),
                    SkinColor = GetRandomColor(),
                    BackgroundColor = GetRandomBackgroundColor(),
                    LevelUpTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    SkinColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    BackgroundColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    PointsChangedColor = _random.Next(0, 5000),
                    PointsChangedTimeSkinColor = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    PointsChangedBackgroundColor = _random.Next(0, 5000),
                    PointsGainedLevelUp = _random.Next(0, 1000),
                    PointsGainedTimeLevelUp = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.Pets.AddRange(pets);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCouponTypesAsync()
        {
            var existingCount = await _context.CouponTypes.CountAsync();
            if (existingCount >= 200) return;

            var couponTypes = new List<CouponType>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                couponTypes.Add(new CouponType
                {
                    Name = $"優惠券類型{i}",
                    DiscountType = _random.Next(2) == 0 ? "Amount" : "Percent",
                    DiscountValue = _random.Next(2) == 0 ? _random.Next(10, 500) : (decimal)(_random.Next(5, 95) / 100.0),
                    MinSpend = _random.Next(0, 2000),
                    ValidFrom = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    ValidTo = DateTime.UtcNow.AddDays(_random.Next(30, 365)),
                    PointsCost = _random.Next(100, 2000),
                    Description = $"優惠券類型{i}的描述"
                });
            }
            _context.CouponTypes.AddRange(couponTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEVoucherTypesAsync()
        {
            var existingCount = await _context.EVoucherTypes.CountAsync();
            if (existingCount >= 200) return;

            var evoucherTypes = new List<EVoucherType>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                evoucherTypes.Add(new EVoucherType
                {
                    Name = $"禮券類型{i}",
                    ValueAmount = _random.Next(100, 5000),
                    ValidFrom = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    ValidTo = DateTime.UtcNow.AddDays(_random.Next(30, 365)),
                    PointsCost = _random.Next(500, 5000),
                    TotalAvailable = _random.Next(100, 1000),
                    Description = $"禮券類型{i}的描述"
                });
            }
            _context.EVoucherTypes.AddRange(evoucherTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCouponsAsync()
        {
            var existingCount = await _context.Coupons.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var couponTypes = await _context.CouponTypes.ToListAsync();

            var coupons = new List<Coupon>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                var couponType = couponTypes[_random.Next(couponTypes.Count)];

                coupons.Add(new Coupon
                {
                    UserId = user.UserId,
                    CouponTypeId = couponType.CouponTypeId,
                    CouponCode = GenerateCouponCode(),
                    IsUsed = _random.Next(2) == 0,
                    AcquiredTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    UsedTime = _random.Next(2) == 0 ? DateTime.UtcNow.AddDays(-_random.Next(30)) : null,
                    UsedInOrderId = _random.Next(2) == 0 ? _random.Next(1, 1000) : null
                });
            }
            _context.Coupons.AddRange(coupons);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEVouchersAsync()
        {
            var existingCount = await _context.EVouchers.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var evoucherTypes = await _context.EVoucherTypes.ToListAsync();

            var evouchers = new List<EVoucher>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                var evoucherType = evoucherTypes[_random.Next(evoucherTypes.Count)];

                evouchers.Add(new EVoucher
                {
                    UserId = user.UserId,
                    EVoucherTypeId = evoucherType.EVoucherTypeId,
                    EVoucherCode = GenerateEVoucherCode(),
                    IsUsed = _random.Next(2) == 0,
                    AcquiredTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    UsedTime = _random.Next(2) == 0 ? DateTime.UtcNow.AddDays(-_random.Next(30)) : null
                });
            }
            _context.EVouchers.AddRange(evouchers);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserSignInStatsAsync()
        {
            var existingCount = await _context.UserSignInStats.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var signInStats = new List<UserSignInStats>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                signInStats.Add(new UserSignInStats
                {
                    UserId = user.UserId,
                    SignTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    PointsChanged = _random.Next(10, 100),
                    PointsChangedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    ExpGained = _random.Next(0, 50),
                    ExpGainedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    CouponGained = _random.Next(0, 5).ToString(),
                    CouponGainedTime = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.UserSignInStats.AddRange(signInStats);
            await _context.SaveChangesAsync();
        }

        private async Task SeedMiniGamesAsync()
        {
            var existingCount = await _context.MiniGames.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var pets = await _context.Pets.ToListAsync();
            var results = new[] { "Win", "Lose", "Abort" };

            var miniGames = new List<MiniGame>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                var pet = pets[_random.Next(pets.Count)];
                var result = results[_random.Next(results.Length)];

                miniGames.Add(new MiniGame
                {
                    UserId = user.UserId,
                    PetId = pet.PetId,
                    Level = _random.Next(1, 10),
                    MonsterCount = _random.Next(5, 15),
                    SpeedMultiplier = (decimal)(_random.Next(100, 300) / 100.0),
                    Result = result,
                    ExpGained = result == "Win" ? _random.Next(10, 100) : _random.Next(0, 20),
                    ExpGainedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    PointsGained = result == "Win" ? _random.Next(10, 100) : _random.Next(0, 20),
                    PointsGainedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    CouponGained = result == "Win" && _random.Next(2) == 0 ? _random.Next(1, 3).ToString() : "0",
                    CouponGainedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    HungerDelta = _random.Next(-30, 10),
                    MoodDelta = _random.Next(-20, 30),
                    StaminaDelta = _random.Next(-30, 10),
                    CleanlinessDelta = _random.Next(-20, 10),
                    StartTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    Aborted = result == "Abort"
                });
            }
            _context.MiniGames.AddRange(miniGames);
            await _context.SaveChangesAsync();
        }

        private async Task SeedWalletHistoriesAsync()
        {
            var existingCount = await _context.WalletHistories.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var changeTypes = new[] { "Point", "Coupon", "EVoucher" };
            var descriptions = new[] { "每日簽到獲得", "小遊戲勝利獲得", "寵物升級獲得", "兌換優惠券", "兌換禮券" };

            var walletHistories = new List<WalletHistory>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                var changeType = changeTypes[_random.Next(changeTypes.Length)];
                var description = descriptions[_random.Next(descriptions.Length)];

                walletHistories.Add(new WalletHistory
                {
                    UserId = user.UserId,
                    ChangeType = changeType,
                    PointsChanged = _random.Next(-500, 500),
                    ItemCode = _random.Next(2) == 0 ? GenerateCouponCode() : null,
                    Description = $"{description}{_random.Next(10, 100)}點",
                    ChangeTime = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.WalletHistories.AddRange(walletHistories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserIntroducesAsync()
        {
            var existingCount = await _context.UserIntroduces.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var genders = new[] { "男", "女", "其他" };
            var addresses = new[] { "台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市" };

            var userIntroduces = new List<UserIntroduce>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                userIntroduces.Add(new UserIntroduce
                {
                    UserId = user.UserId,
                    UserNickName = $"暱稱{i}",
                    Gender = genders[_random.Next(genders.Length)],
                    IdNumber = GenerateIdNumber(),
                    Cellphone = $"09{i:D8}",
                    Email = user.UserEmail ?? $"user{i}@example.com",
                    Address = $"{addresses[_random.Next(addresses.Length)]}第{i}街{i}號",
                    DateOfBirth = DateTime.UtcNow.AddYears(-_random.Next(18, 80)),
                    CreateAccount = user.CreatedAt,
                    UserIntroduceText = $"這是用戶{i}的自我介紹"
                });
            }
            _context.UserIntroduces.AddRange(userIntroduces);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserRightsAsync()
        {
            var existingCount = await _context.UserRights.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var userRights = new List<UserRights>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                userRights.Add(new UserRights
                {
                    UserId = user.UserId,
                    UserStatus = _random.Next(10) != 0, // 90% 正常狀態
                    ShoppingPermission = _random.Next(10) != 0,
                    MessagePermission = _random.Next(10) != 0,
                    SalesAuthority = _random.Next(5) == 0 // 20% 有銷售權限
                });
            }
            _context.UserRights.AddRange(userRights);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserWalletsAsync()
        {
            var existingCount = await _context.UserWallets.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var userWallets = new List<UserWallet>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                userWallets.Add(new UserWallet
                {
                    UserId = user.UserId,
                    UserPoint = _random.Next(0, 10000)
                });
            }
            _context.UserWallets.AddRange(userWallets);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserTokensAsync()
        {
            var existingCount = await _context.UserTokens.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var providers = new[] { "Google", "Facebook", "Discord", "Local" };

            var userTokens = new List<UserTokens>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                userTokens.Add(new UserTokens
                {
                    UserId = user.UserId,
                    Token = GenerateToken(),
                    Provider = providers[_random.Next(providers.Length)],
                    ExpiresAt = DateTime.UtcNow.AddDays(_random.Next(1, 30)),
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.UserTokens.AddRange(userTokens);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserSalesInformationsAsync()
        {
            var existingCount = await _context.UserSalesInformations.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var userSalesInformations = new List<UserSalesInformation>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[_random.Next(users.Count)];
                userSalesInformations.Add(new UserSalesInformation
                {
                    UserId = user.UserId,
                    SalesName = $"銷售員{i}",
                    SalesPhone = $"09{i:D8}",
                    SalesEmail = $"sales{i}@example.com",
                    SalesAddress = $"銷售地址{i}",
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }
            _context.UserSalesInformations.AddRange(userSalesInformations);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEVoucherTokensAsync()
        {
            var existingCount = await _context.EVoucherTokens.CountAsync();
            if (existingCount >= 200) return;

            var evouchers = await _context.EVouchers.ToListAsync();
            var evoucherTokens = new List<EVoucherToken>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var evoucher = evouchers[_random.Next(evouchers.Count)];
                evoucherTokens.Add(new EVoucherToken
                {
                    EVoucherId = evoucher.EVoucherId,
                    Token = GenerateToken(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_random.Next(5, 60)),
                    IsRevoked = _random.Next(10) == 0
                });
            }
            _context.EVoucherTokens.AddRange(evoucherTokens);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEVoucherRedeemLogsAsync()
        {
            var existingCount = await _context.EVoucherRedeemLogs.CountAsync();
            if (existingCount >= 200) return;

            var evouchers = await _context.EVouchers.ToListAsync();
            var evoucherTokens = await _context.EVoucherTokens.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var statuses = new[] { "Approved", "Rejected", "Expired", "AlreadyUsed", "Revoked" };

            var evoucherRedeemLogs = new List<EVoucherRedeemLog>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var evoucher = evouchers[_random.Next(evouchers.Count)];
                var evoucherToken = evoucherTokens[_random.Next(evoucherTokens.Count)];
                var user = users[_random.Next(users.Count)];
                var status = statuses[_random.Next(statuses.Length)];

                evoucherRedeemLogs.Add(new EVoucherRedeemLog
                {
                    EVoucherId = evoucher.EVoucherId,
                    TokenId = evoucherToken.TokenId,
                    UserId = user.UserId,
                    ScannedAt = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    Status = status
                });
            }
            _context.EVoucherRedeemLogs.AddRange(evoucherRedeemLogs);
            await _context.SaveChangesAsync();
        }

        private string GenerateCouponCode()
        {
            return $"COUPON{Random.Shared.Next(100000, 999999)}";
        }

        private string GenerateEVoucherCode()
        {
            return $"EVOUCHER{Random.Shared.Next(100000, 999999)}";
        }

        private string GenerateIdNumber()
        {
            return $"{Random.Shared.Next(100000000, 999999999)}";
        }

        private string GenerateToken()
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        }

        private string GetRandomColor()
        {
            var colors = new[] { "#ADD8E6", "#FFB6C1", "#98FB98", "#F0E68C", "#DDA0DD", "#FFA07A", "#87CEEB", "#F5DEB3" };
            return colors[_random.Next(colors.Length)];
        }

        private string GetRandomBackgroundColor()
        {
            var colors = new[] { "粉藍", "粉紅", "粉綠", "粉黃", "粉紫", "粉橙", "粉灰", "粉白" };
            return colors[_random.Next(colors.Length)];
        }
    }
}