using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Services
{
    /// <summary>
    /// 錢包讀取服務測試
    /// 測試錢包聚合查詢、歷史記錄與券類列表功能
    /// </summary>
    public class WalletReadServiceTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public WalletReadServiceTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        [Fact]
        public async Task GetWalletAggregate_WithUserData_ReturnsCorrectSummary()
        {
            // Arrange
            var testUser = new User
            {
                UserID = 1,
                UserName = "testuser",
                UserAccount = "testuser",
                UserPassword = "hashedpassword"
            };
            await _context.Users.AddAsync(testUser);

            var testWallet = new UserWallet
            {
                UserID = testUser.UserID,
                UserPoint = 1500
            };
            await _context.UserWallets.AddAsync(testWallet);

            // 新增優惠券類型
            var couponType = new CouponType
            {
                CouponTypeID = 1,
                Name = "測試優惠券",
                DiscountType = "Amount",
                DiscountValue = 100,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(30),
                PointsCost = 200
            };
            await _context.CouponTypes.AddAsync(couponType);

            // 新增優惠券
            var coupon = new Coupon
            {
                CouponID = 1,
                CouponCode = "TEST001",
                CouponTypeID = couponType.CouponTypeID,
                UserID = testUser.UserID,
                IsUsed = false,
                AcquiredTime = DateTime.UtcNow
            };
            await _context.Coupons.AddAsync(coupon);

            // 新增錢包歷史記錄
            var walletHistory = new WalletHistory
            {
                UserID = testUser.UserID,
                ChangeType = "Point",
                PointsChanged = 500,
                Description = "簽到獎勵",
                ChangeTime = DateTime.UtcNow
            };
            await _context.WalletHistories.AddAsync(walletHistory);

            await _context.SaveChangesAsync();

            // Act - 模擬錢包聚合查詢
            var walletData = await _context.UserWallets
                .AsNoTracking()
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserID == testUser.UserID);

            var activeCouponsCount = await _context.Coupons
                .AsNoTracking()
                .CountAsync(c => c.UserID == testUser.UserID && !c.IsUsed);

            var recentHistoryCount = await _context.WalletHistories
                .AsNoTracking()
                .CountAsync(h => h.UserID == testUser.UserID);

            // Assert
            Assert.NotNull(walletData);
            Assert.Equal(1500, walletData.UserPoint);
            Assert.Equal("testuser", walletData.User.UserName);
            Assert.Equal(1, activeCouponsCount);
            Assert.Equal(1, recentHistoryCount);
        }

        [Fact]
        public async Task GetWalletHistory_WithFiltering_ReturnsFilteredResults()
        {
            // Arrange
            var testUserId = 1;
            var histories = new List<WalletHistory>
            {
                new WalletHistory
                {
                    UserID = testUserId,
                    ChangeType = "Point",
                    PointsChanged = 100,
                    Description = "簽到獎勵",
                    ChangeTime = DateTime.UtcNow.AddDays(-1)
                },
                new WalletHistory
                {
                    UserID = testUserId,
                    ChangeType = "Coupon",
                    PointsChanged = -200,
                    ItemCode = "DISC001",
                    Description = "兌換優惠券",
                    ChangeTime = DateTime.UtcNow.AddDays(-2)
                },
                new WalletHistory
                {
                    UserID = testUserId,
                    ChangeType = "Point",
                    PointsChanged = 50,
                    Description = "遊戲獎勵",
                    ChangeTime = DateTime.UtcNow.AddDays(-3)
                }
            };

            await _context.WalletHistories.AddRangeAsync(histories);
            await _context.SaveChangesAsync();

            // Act - 查詢點數類型的歷史記錄
            var pointHistories = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserID == testUserId && h.ChangeType == "Point")
                .OrderByDescending(h => h.ChangeTime)
                .ToListAsync();

            // Act - 查詢最近的記錄（分頁測試）
            var recentHistories = await _context.WalletHistories
                .AsNoTracking()
                .Where(h => h.UserID == testUserId)
                .OrderByDescending(h => h.ChangeTime)
                .Take(2)
                .ToListAsync();

            // Assert
            Assert.Equal(2, pointHistories.Count);
            Assert.All(pointHistories, h => Assert.Equal("Point", h.ChangeType));
            Assert.True(pointHistories[0].PointsChanged > 0); // 最新的應該是簽到獎勵

            Assert.Equal(2, recentHistories.Count);
            Assert.Equal("簽到獎勵", recentHistories[0].Description); // 最新的記錄
        }

        [Fact]
        public async Task GetCouponsList_WithTypeFiltering_ReturnsCorrectCoupons()
        {
            // Arrange
            var testUserId = 1;
            
            // 創建優惠券類型
            var couponTypes = new List<CouponType>
            {
                new CouponType
                {
                    CouponTypeID = 1,
                    Name = "折扣券",
                    DiscountType = "Percent",
                    DiscountValue = 0.1m,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddDays(30),
                    PointsCost = 100
                },
                new CouponType
                {
                    CouponTypeID = 2,
                    Name = "免運券",
                    DiscountType = "Amount",
                    DiscountValue = 60,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddDays(15),
                    PointsCost = 80
                }
            };
            await _context.CouponTypes.AddRangeAsync(couponTypes);

            // 創建優惠券
            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    CouponID = 1,
                    CouponCode = "DISC001",
                    CouponTypeID = 1,
                    UserID = testUserId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                },
                new Coupon
                {
                    CouponID = 2,
                    CouponCode = "FREE001",
                    CouponTypeID = 2,
                    UserID = testUserId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow.AddHours(-1)
                },
                new Coupon
                {
                    CouponID = 3,
                    CouponCode = "USED001",
                    CouponTypeID = 1,
                    UserID = testUserId,
                    IsUsed = true,
                    AcquiredTime = DateTime.UtcNow.AddDays(-1),
                    UsedTime = DateTime.UtcNow.AddHours(-2)
                }
            };
            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();

            // Act - 查詢可用的優惠券
            var availableCoupons = await _context.Coupons
                .AsNoTracking()
                .Include(c => c.CouponType)
                .Where(c => c.UserID == testUserId && !c.IsUsed)
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            // Act - 查詢特定類型的優惠券
            var discountCoupons = await _context.Coupons
                .AsNoTracking()
                .Include(c => c.CouponType)
                .Where(c => c.UserID == testUserId && c.CouponTypeID == 1)
                .ToListAsync();

            // Assert
            Assert.Equal(2, availableCoupons.Count);
            Assert.All(availableCoupons, c => Assert.False(c.IsUsed));
            Assert.Equal("DISC001", availableCoupons[0].CouponCode); // 最新獲得的

            Assert.Equal(2, discountCoupons.Count); // 包含已使用的
            Assert.All(discountCoupons, c => Assert.Equal("折扣券", c.CouponType.Name));
        }

        [Fact]
        public async Task GetEVouchersList_WithStatusFiltering_ReturnsCorrectEVouchers()
        {
            // Arrange
            var testUserId = 1;
            
            // 創建電子禮券類型
            var evoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "500元禮券",
                ValueAmount = 500,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(90),
                PointsCost = 800,
                TotalAvailable = 100
            };
            await _context.EVoucherTypes.AddAsync(evoucherType);

            // 創建電子禮券
            var evouchers = new List<EVoucher>
            {
                new EVoucher
                {
                    EVoucherID = 1,
                    EVoucherCode = "EV001",
                    EVoucherTypeID = evoucherType.EVoucherTypeID,
                    UserID = testUserId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                },
                new EVoucher
                {
                    EVoucherID = 2,
                    EVoucherCode = "EV002",
                    EVoucherTypeID = evoucherType.EVoucherTypeID,
                    UserID = testUserId,
                    IsUsed = true,
                    AcquiredTime = DateTime.UtcNow.AddDays(-5),
                    UsedTime = DateTime.UtcNow.AddDays(-1)
                }
            };
            await _context.EVouchers.AddRangeAsync(evouchers);
            await _context.SaveChangesAsync();

            // Act - 查詢可用的電子禮券
            var availableEVouchers = await _context.EVouchers
                .AsNoTracking()
                .Include(e => e.EVoucherType)
                .Where(e => e.UserID == testUserId && !e.IsUsed)
                .ToListAsync();

            // Act - 查詢所有電子禮券（包含已使用）
            var allEVouchers = await _context.EVouchers
                .AsNoTracking()
                .Include(e => e.EVoucherType)
                .Where(e => e.UserID == testUserId)
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            // Assert
            Assert.Single(availableEVouchers);
            Assert.Equal("EV001", availableEVouchers[0].EVoucherCode);
            Assert.False(availableEVouchers[0].IsUsed);

            Assert.Equal(2, allEVouchers.Count);
            Assert.Equal("EV001", allEVouchers[0].EVoucherCode); // 最新獲得的
            Assert.Equal(500, allEVouchers[0].EVoucherType.ValueAmount);
        }
    }
}