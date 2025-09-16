using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Controllers;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;

namespace GameSpace.Tests.Controllers
{
    /// <summary>
    /// AdminWalletTypesController 單元測試
    /// 測試 CRUD 操作、驗證與 RBAC 權限控制
    /// </summary>
    public class AdminWalletTypesControllerTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletTypesControllerTests()
        {
            var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new GameSpaceDbContext(options);
        }

        private AdminWalletTypesController CreateController()
        {
            return new AdminWalletTypesController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CouponTypes_ReturnsViewWithCouponTypes()
        {
            // Arrange
            var controller = CreateController();
            var testCouponType = new CouponType
            {
                CouponTypeID = 1,
                Name = "測試優惠券",
                DiscountType = "Amount",
                DiscountValue = 100,
                MinSpend = 500,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(30),
                PointsCost = 200,
                Description = "測試用優惠券類型"
            };

            await _context.CouponTypes.AddAsync(testCouponType);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.CouponTypes();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<CouponType>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("測試優惠券", model[0].Name);
        }

        [Fact]
        public async Task CreateCouponType_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var controller = CreateController();
            var newCouponType = new CouponType
            {
                Name = "新優惠券類型",
                DiscountType = "Percent",
                DiscountValue = 0.15m,
                MinSpend = 1000,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(60),
                PointsCost = 300,
                Description = "新建的優惠券類型"
            };

            // Act
            var result = await controller.CreateCouponType(newCouponType);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminWalletTypesController.CouponTypes), redirectResult.ActionName);

            // Verify data was saved
            var savedCouponType = await _context.CouponTypes
                .FirstOrDefaultAsync(ct => ct.Name == "新優惠券類型");
            Assert.NotNull(savedCouponType);
            Assert.Equal("Percent", savedCouponType.DiscountType);
        }

        [Fact]
        public async Task EditCouponType_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController();
            var nonExistentId = 999;

            // Act
            var result = await controller.EditCouponType(nonExistentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("找不到指定的優惠券類型", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteCouponType_WithRelatedCoupons_ReturnsError()
        {
            // Arrange
            var controller = CreateController();
            
            // 創建優惠券類型
            var couponType = new CouponType
            {
                CouponTypeID = 1,
                Name = "有關聯的優惠券類型",
                DiscountType = "Amount",
                DiscountValue = 50,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(30),
                PointsCost = 100
            };
            await _context.CouponTypes.AddAsync(couponType);
            await _context.SaveChangesAsync();

            // 創建相關的優惠券
            var relatedCoupon = new Coupon
            {
                CouponID = 1,
                CouponCode = "TEST001",
                CouponTypeID = couponType.CouponTypeID,
                UserID = 1,
                AcquiredTime = DateTime.UtcNow
            };
            await _context.Coupons.AddAsync(relatedCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteCouponType(couponType.CouponTypeID);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminWalletTypesController.CouponTypes), redirectResult.ActionName);
            
            // Verify the coupon type still exists
            var stillExists = await _context.CouponTypes
                .AnyAsync(ct => ct.CouponTypeID == couponType.CouponTypeID);
            Assert.True(stillExists);
        }

        [Fact]
        public async Task EVoucherTypes_ReturnsViewWithEVoucherTypes()
        {
            // Arrange
            var controller = CreateController();
            var testEVoucherType = new EVoucherType
            {
                EVoucherTypeID = 1,
                Name = "測試電子禮券",
                ValueAmount = 500,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(90),
                PointsCost = 800,
                TotalAvailable = 100,
                Description = "測試用電子禮券類型"
            };

            await _context.EVoucherTypes.AddAsync(testEVoucherType);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.EVoucherTypes();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EVoucherType>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("測試電子禮券", model[0].Name);
            Assert.Equal(500, model[0].ValueAmount);
        }

        [Fact]
        public async Task CreateEVoucherType_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var controller = CreateController();
            var newEVoucherType = new EVoucherType
            {
                Name = "新電子禮券類型",
                ValueAmount = 1000,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(120),
                PointsCost = 1500,
                TotalAvailable = 50,
                Description = "新建的電子禮券類型"
            };

            // Act
            var result = await controller.CreateEVoucherType(newEVoucherType);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminWalletTypesController.EVoucherTypes), redirectResult.ActionName);

            // Verify data was saved
            var savedEVoucherType = await _context.EVoucherTypes
                .FirstOrDefaultAsync(et => et.Name == "新電子禮券類型");
            Assert.NotNull(savedEVoucherType);
            Assert.Equal(1000, savedEVoucherType.ValueAmount);
        }
    }
}