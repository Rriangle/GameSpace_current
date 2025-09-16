using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using Xunit;
using System.Reflection;

namespace GameSpace.Tests.Services
{
    /// <summary>
    /// 欄位覆蓋測試
    /// 驗證 DTO 欄位與 database.json 結構對應
    /// </summary>
    public class FieldCoverageTests : IDisposable
    {
        private readonly GameSpaceDbContext _context;

        public FieldCoverageTests()
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
        public void UserWallet_ModelProperties_AlignWithDatabaseSchema()
        {
            // Arrange - 根據 database.json User_Wallet 表定義
            var expectedColumns = new[] { "User_Id", "User_Point" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(UserWallet);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證必要欄位存在且有正確的 Column 屬性
            var userIdProperty = properties.FirstOrDefault(p => p.Name == "UserID");
            Assert.NotNull(userIdProperty);
            
            var userPointProperty = properties.FirstOrDefault(p => p.Name == "UserPoint");
            Assert.NotNull(userPointProperty);
            
            // 驗證 Column 屬性
            var userIdColumn = userIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            Assert.NotNull(userIdColumn);
            Assert.Equal("User_Id", userIdColumn.Name);
            
            var userPointColumn = userPointProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            Assert.NotNull(userPointColumn);
            Assert.Equal("User_Point", userPointColumn.Name);
        }

        [Fact]
        public void EVoucherToken_ModelProperties_AlignWithDatabaseSchema()
        {
            // Arrange - 根據 database.json EVoucherToken 表定義
            var expectedColumns = new[] { "TokenID", "EVoucherID", "Token", "ExpiresAt", "IsRevoked" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(EVoucherToken);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證關鍵欄位存在
            Assert.Contains(properties, p => p.Name == "TokenID");
            Assert.Contains(properties, p => p.Name == "EVoucherID");
            Assert.Contains(properties, p => p.Name == "Token");
            Assert.Contains(properties, p => p.Name == "ExpiresAt");
            Assert.Contains(properties, p => p.Name == "IsRevoked");
            
            // 驗證主鍵 Column 屬性
            var tokenIdProperty = properties.First(p => p.Name == "TokenID");
            var columnAttr = tokenIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            Assert.NotNull(columnAttr);
            Assert.Equal("TokenID", columnAttr.Name);
        }

        [Fact]
        public void EVoucherRedeemLog_ModelProperties_AlignWithDatabaseSchema()
        {
            // Arrange - 根據 database.json EVoucherRedeemLog 表定義
            var expectedColumns = new[] { "RedeemID", "EVoucherID", "TokenID", "UserID", "ScannedAt", "Status" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(EVoucherRedeemLog);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證關鍵欄位存在
            Assert.Contains(properties, p => p.Name == "RedeemID");
            Assert.Contains(properties, p => p.Name == "EVoucherID");
            Assert.Contains(properties, p => p.Name == "TokenID");
            Assert.Contains(properties, p => p.Name == "UserID");
            Assert.Contains(properties, p => p.Name == "ScannedAt");
            Assert.Contains(properties, p => p.Name == "Status");
            
            // 驗證主鍵 Column 屬性
            var redeemIdProperty = properties.First(p => p.Name == "RedeemID");
            var columnAttr = redeemIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
            Assert.NotNull(columnAttr);
            Assert.Equal("RedeemID", columnAttr.Name);
        }

        [Fact]
        public void UserSignInStats_ModelProperties_AlignWithDatabaseSchema()
        {
            // Arrange - 根據 database.json UserSignInStats 表定義
            var expectedColumns = new[] { "LogID", "SignTime", "UserID", "PointsGained", "PointsGainedTime", "ExpGained", "ExpGainedTime", "CouponGained", "CouponGainedTime" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(UserSignInStats);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證所有必要欄位存在
            foreach (var expectedColumn in expectedColumns)
            {
                Assert.Contains(properties, p => p.Name == expectedColumn);
            }
            
            // 特別驗證時間戳欄位
            var pointsGainedTimeProperty = properties.First(p => p.Name == "PointsGainedTime");
            Assert.Equal(typeof(DateTime), pointsGainedTimeProperty.PropertyType);
            
            var expGainedTimeProperty = properties.First(p => p.Name == "ExpGainedTime");
            Assert.Equal(typeof(DateTime), expGainedTimeProperty.PropertyType);
            
            var couponGainedTimeProperty = properties.First(p => p.Name == "CouponGainedTime");
            Assert.Equal(typeof(DateTime), couponGainedTimeProperty.PropertyType);
        }

        [Fact]
        public void Pet_ModelProperties_IncludePointsChangedFields()
        {
            // Arrange - 根據 database.json Pet 表定義
            var expectedPointsFields = new[] { "PointsChanged_SkinColor", "PointsChangedTime_SkinColor", "PointsChanged_BackgroundColor", "PointsGained_LevelUp", "PointsGainedTime_LevelUp" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(Pet);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證點數相關欄位存在
            foreach (var expectedField in expectedPointsFields)
            {
                Assert.Contains(properties, p => p.Name == expectedField);
            }
            
            // 驗證資料類型
            var pointsChangedSkinProperty = properties.First(p => p.Name == "PointsChanged_SkinColor");
            Assert.Equal(typeof(int), pointsChangedSkinProperty.PropertyType);
            
            var pointsGainedLevelUpProperty = properties.First(p => p.Name == "PointsGained_LevelUp");
            Assert.Equal(typeof(int), pointsGainedLevelUpProperty.PropertyType);
        }

        [Fact]
        public void MiniGame_ModelProperties_IncludePointsGainedFields()
        {
            // Arrange - 根據 database.json MiniGame 表定義
            var expectedPointsFields = new[] { "PointsGained", "PointsGainedTime" };
            
            // Act - 檢查 Model 屬性
            var modelType = typeof(MiniGame);
            var properties = modelType.GetProperties();
            
            // Assert - 驗證點數獎勵欄位存在
            foreach (var expectedField in expectedPointsFields)
            {
                Assert.Contains(properties, p => p.Name == expectedField);
            }
            
            // 驗證資料類型
            var pointsGainedProperty = properties.First(p => p.Name == "PointsGained");
            Assert.Equal(typeof(int), pointsGainedProperty.PropertyType);
            
            var pointsGainedTimeProperty = properties.First(p => p.Name == "PointsGainedTime");
            Assert.Equal(typeof(DateTime), pointsGainedTimeProperty.PropertyType);
        }
    }
}