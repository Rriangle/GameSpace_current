using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Linq;

namespace GameSpace.Services
{
    public class DataSeedingService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly Random _random;

        public DataSeedingService(GameSpacedatabaseContext context)
        {
            _context = context;
            _random = new Random(12345); // 固定種子以確保可重現性
        }

        public async Task SeedDataAsync()
        {
            // 先種子基本資料
            await SeedCouponTypes();
            await SeedEvoucherTypes();
            await SeedUsers();
            await SeedUserIntroduces();
            await SeedUserRights();
            await SeedUserWallets();
            await SeedPets();
            
            // 再種子操作記錄
            await SeedUserSignInStats();
            await SeedMiniGames();
            await SeedWalletHistory();
            await SeedCoupons();
            await SeedEvouchers();
            await SeedEvoucherTokens();
            await SeedEvoucherRedeemLogs();
        }

        private async Task SeedUsers()
        {
            var existingCount = await _context.Users.CountAsync();
            var targetCount = 200;
            if (existingCount >= targetCount) return;

            var users = new List<User>();
            var names = new[] { "王小明", "李小華", "張美玲", "陳志強", "林淑芬", "黃俊傑", "吳雅婷", "劉建國", "蔡文雄", "楊麗華" };
            
            for (int i = existingCount + 1; i <= targetCount; i++)
            {
                users.Add(new User
                {
                    UserName = $"{names[_random.Next(names.Length)]}{i:D3}",
                    Email = $"user{i}@example.com",
                    // 使用User模型的實際屬性
                    NormalizedUserName = $"USER{i}@EXAMPLE.COM",
                    NormalizedEmail = $"USER{i}@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                });
            }

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPets()
        {
            var existingCount = await _context.Pets.CountAsync();
            var targetCount = 200;
            if (existingCount >= targetCount) return;

            var pets = new List<Pet>();
            var petNames = new[] { "小可愛", "毛球", "史萊姆", "小精靈", "寶貝", "哆啦A夢", "皮卡丘", "小火龍", "傑尼龜", "妙蛙種子" };
            var colors = new[] { "#ADD8E6", "#FFB6C1", "#98FB98", "#DDA0DD", "#F0E68C", "#87CEEB", "#FFA07A", "#20B2AA", "#9370DB", "#3CB371" };
            var backgrounds = new[] { "粉藍", "粉紅", "淺綠", "淺紫", "淺黃", "天藍", "珊瑚", "海藍", "紫色", "森林綠" };
            
            for (int i = existingCount + 1; i <= targetCount; i++)
            {
                var level = _random.Next(1, 51);
                pets.Add(new Pet
                {
                    UserId = Math.Min(i, await _context.Users.CountAsync()), // 確保UserId存在
                    PetName = $"{petNames[_random.Next(petNames.Length)]}{i:D3}",
                    Level = level,
                    LevelUpTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    Experience = _random.Next(0, CalculateRequiredExp(level)),
                    Hunger = _random.Next(0, 101),
                    Mood = _random.Next(0, 101),
                    Stamina = _random.Next(0, 101),
                    Cleanliness = _random.Next(0, 101),
                    Health = _random.Next(50, 101), // 健康值不會太低
                    SkinColor = colors[_random.Next(colors.Length)],
                    SkinColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    BackgroundColor = backgrounds[_random.Next(backgrounds.Length)],
                    BackgroundColorChangedTime = DateTime.UtcNow.AddDays(-_random.Next(30)),
                    PointsChangedSkinColor = _random.Next(0, 5) == 0 ? 2000 : 0, // 20%機率曾換過膚色
                    PointsChangedBackgroundColor = 0, // 預設免費
                    PointsGainedLevelUp = level > 1 ? (level / 10 + 1) * 10 : 0,
                    PointsGainedTimeLevelUp = DateTime.UtcNow.AddDays(-_random.Next(30))
                });
            }

            _context.Pets.AddRange(pets);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserWallets()
        {
            var existingCount = await _context.UserWallets.CountAsync();
            var targetCount = 200;
            if (existingCount >= targetCount) return;

            var wallets = new List<UserWallet>();
            var maxUserId = await _context.Users.CountAsync();
            
            for (int i = existingCount + 1; i <= targetCount; i++)
            {
                if (i <= maxUserId) // 確保每個用戶都有錢包
                {
                    wallets.Add(new UserWallet
                    {
                        UserId = i,
                        UserPoint = _random.Next(100, 10000) // 給予初始點數
                    });
                }
            }

            _context.UserWallets.AddRange(wallets);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserSignInStats()
        {
            var existingCount = await _context.UserSignInStat.CountAsync();
            if (existingCount >= 200) return;

            var signInStats = new List<UserSignInStat>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                signInStats.Add(new UserSignInStat
                {
                    UserID = _random.Next(1, 201),
                    SignTime = DateTime.Now.AddDays(-_random.Next(30)),
                    Point = _random.Next(0, 100)
                });
            }

            _context.UserSignInStat.AddRange(signInStats);
            await _context.SaveChangesAsync();
        }

        private async Task SeedMiniGames()
        {
            var existingCount = await _context.MiniGame.CountAsync();
            if (existingCount >= 200) return;

            var gameTypes = new[] { "Memory", "Puzzle", "Quiz", "Action", "Strategy" };
            var miniGames = new List<MiniGame>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                miniGames.Add(new MiniGame
                {
                    UserID = _random.Next(1, 201),
                    GameType = gameTypes[_random.Next(gameTypes.Length)],
                    StartTime = DateTime.Now.AddDays(-_random.Next(30)),
                    EndTime = DateTime.Now.AddDays(-_random.Next(30)).AddMinutes(_random.Next(1, 60)),
                    Score = _random.Next(0, 1000),
                    Point = _random.Next(0, 100)
                });
            }

            _context.MiniGame.AddRange(miniGames);
            await _context.SaveChangesAsync();
        }

        private async Task SeedWalletHistory()
        {
            var existingCount = await _context.WalletHistory.CountAsync();
            if (existingCount >= 200) return;

            var transactionTypes = new[] { "Earn", "Spend", "Bonus", "Refund" };
            var walletHistory = new List<WalletHistory>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                walletHistory.Add(new WalletHistory
                {
                    UserID = _random.Next(1, 201),
                    TransactionType = transactionTypes[_random.Next(transactionTypes.Length)],
                    Amount = _random.Next(1, 1000),
                    TransactionTime = DateTime.Now.AddDays(-_random.Next(30)),
                    Description = $"Transaction {i}"
                });
            }

            _context.WalletHistory.AddRange(walletHistory);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCouponTypes()
        {
            var existingCount = await _context.CouponTypes.CountAsync();
            if (existingCount >= 10) return; // 只需要基本的優惠券類型

            var couponTypes = new List<CouponType>();
            var types = new[]
            {
                new { Name = "85折優惠券", Type = "Percent", Value = 0.15m, Min = 500m, Cost = 500 },
                new { Name = "滿1000折100", Type = "Amount", Value = 100m, Min = 1000m, Cost = 300 },
                new { Name = "免運券", Type = "Amount", Value = 60m, Min = 0m, Cost = 200 },
                new { Name = "簽到獎勵券", Type = "Percent", Value = 0.10m, Min = 300m, Cost = 0 },
                new { Name = "遊戲獎勵券", Type = "Amount", Value = 50m, Min = 200m, Cost = 0 }
            };

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                couponTypes.Add(new CouponType
                {
                    Name = type.Name,
                    DiscountType = type.Type,
                    DiscountValue = type.Value,
                    MinSpend = type.Min,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    PointsCost = type.Cost,
                    Description = $"{type.Name}的詳細說明"
                });
            }

            _context.CouponTypes.AddRange(couponTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCoupons()
        {
            var existingCount = await _context.Coupons.CountAsync();
            if (existingCount >= 200) return;

            var users = await _context.Users.ToListAsync();
            var couponTypes = await _context.CouponTypes.ToListAsync();

            var coupons = new List<Coupon>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                var user = users[Random.Shared.Next(users.Count)];
                var couponType = couponTypes[Random.Shared.Next(couponTypes.Count)];

                coupons.Add(new Coupon
                {
                    UserId = user.UserId,
                    CouponTypeId = couponType.CouponTypeId,
                    CouponCode = GenerateCouponCode(),
                    CouponDescription = $"Description for Coupon{i}",
                    CouponPrice = _random.Next(10, 1000),
                    CouponStatus = _random.Next(2) == 0 ? "Active" : "Inactive",
                    CouponCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    CouponUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.Coupon.AddRange(coupons);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEvoucherTypes()
        {
            var existingCount = await _context.EvoucherType.CountAsync();
            if (existingCount >= 200) return;

            var evoucherTypes = new List<EvoucherType>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                evoucherTypes.Add(new EvoucherType
                {
                    EvoucherTypeName = $"EvoucherType{i}",
                    EvoucherTypeDescription = $"Description for EvoucherType{i}",
                    EvoucherTypeCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    EvoucherTypeUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.EvoucherType.AddRange(evoucherTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEvouchers()
        {
            var existingCount = await _context.Evoucher.CountAsync();
            if (existingCount >= 200) return;

            var evouchers = new List<Evoucher>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                evouchers.Add(new Evoucher
                {
                    EvoucherTypeID = _random.Next(1, 201),
                    EvoucherName = $"Evoucher{i}",
                    EvoucherDescription = $"Description for Evoucher{i}",
                    EvoucherPrice = _random.Next(10, 1000),
                    EvoucherStatus = _random.Next(2) == 0 ? "Active" : "Inactive",
                    EvoucherCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    EvoucherUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.Evoucher.AddRange(evouchers);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEvoucherTokens()
        {
            var existingCount = await _context.EvoucherToken.CountAsync();
            if (existingCount >= 200) return;

            var evoucherTokens = new List<EvoucherToken>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                evoucherTokens.Add(new EvoucherToken
                {
                    EvoucherID = _random.Next(1, 201),
                    UserID = _random.Next(1, 201),
                    TokenValue = GenerateToken(),
                    TokenStatus = _random.Next(2) == 0 ? "Active" : "Used",
                    TokenCreateTime = DateTime.Now.AddDays(-_random.Next(30)),
                    TokenExpireTime = DateTime.Now.AddDays(_random.Next(1, 365))
                });
            }

            _context.EvoucherToken.AddRange(evoucherTokens);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEvoucherRedeemLogs()
        {
            var existingCount = await _context.EvoucherRedeemLog.CountAsync();
            if (existingCount >= 200) return;

            var redeemLogs = new List<EvoucherRedeemLog>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                redeemLogs.Add(new EvoucherRedeemLog
                {
                    EvoucherTokenID = _random.Next(1, 201),
                    UserID = _random.Next(1, 201),
                    RedeemTime = DateTime.Now.AddDays(-_random.Next(30)),
                    RedeemStatus = _random.Next(2) == 0 ? "Success" : "Failed"
                });
            }

            _context.EvoucherRedeemLog.AddRange(redeemLogs);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserIntroduces()
        {
            var existingCount = await _context.UserIntroduce.CountAsync();
            if (existingCount >= 200) return;

            var userIntroduces = new List<UserIntroduce>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                userIntroduces.Add(new UserIntroduce
                {
                    UserID = _random.Next(1, 201),
                    IntroduceUserID = _random.Next(1, 201),
                    IntroduceTime = DateTime.Now.AddDays(-_random.Next(30)),
                    IntroduceStatus = _random.Next(2) == 0 ? "Active" : "Inactive"
                });
            }

            _context.UserIntroduce.AddRange(userIntroduces);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserRights()
        {
            var existingCount = await _context.UserRight.CountAsync();
            if (existingCount >= 200) return;

            var userRights = new List<UserRight>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                userRights.Add(new UserRight
                {
                    UserID = _random.Next(1, 201),
                    RightName = $"Right{i}",
                    RightDescription = $"Description for Right{i}",
                    RightCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    RightUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.UserRight.AddRange(userRights);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserTokens()
        {
            var existingCount = await _context.UserToken.CountAsync();
            if (existingCount >= 200) return;

            var userTokens = new List<UserToken>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                userTokens.Add(new UserToken
                {
                    UserID = _random.Next(1, 201),
                    TokenValue = GenerateToken(),
                    TokenType = _random.Next(2) == 0 ? "Access" : "Refresh",
                    TokenCreateTime = DateTime.Now.AddDays(-_random.Next(30)),
                    TokenExpireTime = DateTime.Now.AddDays(_random.Next(1, 30))
                });
            }

            _context.UserToken.AddRange(userTokens);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserSalesInformations()
        {
            var existingCount = await _context.UserSalesInformation.CountAsync();
            if (existingCount >= 200) return;

            var salesInfos = new List<UserSalesInformation>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                salesInfos.Add(new UserSalesInformation
                {
                    UserID = _random.Next(1, 201),
                    SalesAmount = _random.Next(0, 10000),
                    SalesCount = _random.Next(0, 100),
                    SalesCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    SalesUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.UserSalesInformation.AddRange(salesInfos);
            await _context.SaveChangesAsync();
        }

        // Helper methods
        private string GeneratePhoneNumber()
        {
            return $"09{_random.Next(10000000, 99999999)}";
        }

        private DateTime GenerateRandomDate()
        {
            var start = new DateTime(1990, 1, 1);
            var range = (DateTime.Today - start).Days;
            return start.AddDays(_random.Next(range));
        }

        private string GenerateAddress()
        {
            var cities = new[] { "台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市" };
            var districts = new[] { "中正區", "信義區", "松山區", "萬華區", "大同區" };
            return $"{cities[_random.Next(cities.Length)]}{districts[_random.Next(districts.Length)]}";
        }

        private string GenerateToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
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
            return $"COUPON{Random.Shared.Next(100000, 999999)}";
        }

        private string GenerateEvoucherCode()
        {
            return $"EVOUCHER{Random.Shared.Next(100000, 999999)}";
        }

        private string GenerateIntroduceContent()
        {
            var contents = new[]
            {
                "這是一個很棒的遊戲平台！",
                "推薦給所有喜歡遊戲的朋友",
                "功能豐富，介面友善",
                "客服服務很好，推薦！",
                "遊戲種類多，品質不錯"
            };
            return contents[Random.Shared.Next(contents.Length)];
        }

        private string GenerateRightType()
        {
            var types = new[] { "VIP", "Premium", "Basic", "Trial" };
            return types[Random.Shared.Next(types.Length)];
        }

        private string GenerateTokenType()
        {
            var types = new[] { "Access", "Refresh", "API", "Session" };
            return types[Random.Shared.Next(types.Length)];
        }
    }
}