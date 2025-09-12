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
            _random = new Random(12345); // Fixed seed for reproducibility
        }

        public async Task SeedDataAsync()
        {
            await SeedUsers();
            await SeedPets();
            await SeedUserWallets();
            await SeedUserSignInStats();
            await SeedMiniGames();
            await SeedWalletHistory();
            await SeedCouponTypes();
            await SeedCoupons();
            await SeedEvoucherTypes();
            await SeedEvouchers();
            await SeedEvoucherTokens();
            await SeedEvoucherRedeemLogs();
            await SeedUserIntroduces();
            await SeedUserRights();
            await SeedUserTokens();
            await SeedUserSalesInformations();
        }

        private async Task SeedUsers()
        {
            var existingCount = await _context.Users.CountAsync();
            if (existingCount >= 200) return;

            var users = new List<User>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                users.Add(new User
                {
                    UserName = $"User{i:D3}",
                    UserEmail = $"user{i}@example.com",
                    UserPhone = GeneratePhoneNumber(),
                    UserBirthday = GenerateRandomDate(),
                    UserGender = _random.Next(2) == 0 ? "Male" : "Female",
                    UserAddress = GenerateAddress(),
                    UserCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    UserUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPets()
        {
            var existingCount = await _context.Pet.CountAsync();
            if (existingCount >= 200) return;

            var pets = new List<Pet>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                pets.Add(new Pet
                {
                    UserID = i,
                    PetName = $"Pet{i:D3}",
                    PetLevel = _random.Next(1, 100),
                    PetExp = _random.Next(0, 1000),
                    PetHappiness = _random.Next(0, 101),
                    PetCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    PetUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.Pet.AddRange(pets);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUserWallets()
        {
            var existingCount = await _context.User_Wallet.CountAsync();
            if (existingCount >= 200) return;

            var wallets = new List<User_Wallet>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                wallets.Add(new User_Wallet
                {
                    User_Id = i,
                    User_Point = _random.Next(0, 10000),
                    User_CreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    User_UpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.User_Wallet.AddRange(wallets);
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
            var existingCount = await _context.CouponType.CountAsync();
            if (existingCount >= 200) return;

            var couponTypes = new List<CouponType>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                couponTypes.Add(new CouponType
                {
                    CouponTypeName = $"CouponType{i}",
                    CouponTypeDescription = $"Description for CouponType{i}",
                    CouponTypeCreateTime = DateTime.Now.AddDays(-_random.Next(365)),
                    CouponTypeUpdateTime = DateTime.Now.AddDays(-_random.Next(30))
                });
            }

            _context.CouponType.AddRange(couponTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCoupons()
        {
            var existingCount = await _context.Coupon.CountAsync();
            if (existingCount >= 200) return;

            var coupons = new List<Coupon>();
            for (int i = existingCount + 1; i <= 200; i++)
            {
                coupons.Add(new Coupon
                {
                    CouponTypeID = _random.Next(1, 201),
                    CouponName = $"Coupon{i}",
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
            var cities = new[] { "Taipei", "New Taipei", "Taoyuan", "Taichung", "Tainan", "Kaohsiung" };
            var districts = new[] { "Zhongzheng", "Xinyi", "Songshan", "Wanhua", "Datong" };
            return $"{cities[_random.Next(cities.Length)]} {districts[_random.Next(districts.Length)]} District";
        }

        private string GenerateToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}