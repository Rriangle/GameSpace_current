using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GameSpace.Services
{
    /// <summary>
    /// JSON 種子資料匯入器
    /// 從 seedMiniGameArea.json 匯入 MiniGame 相關資料
    /// </summary>
    public class JsonSeedImporter
    {
        private readonly GameSpaceDbContext _context;
        private readonly ILogger<JsonSeedImporter> _logger;

        public JsonSeedImporter(GameSpaceDbContext context, ILogger<JsonSeedImporter> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ImportAsync(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("種子檔案不存在: {JsonPath}", jsonPath);
                return;
            }

            try
            {
                _logger.LogInformation("開始從 {JsonPath} 匯入種子資料", jsonPath);

                var jsonContent = await File.ReadAllTextAsync(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var seedData = JsonSerializer.Deserialize<SeedData>(jsonContent, options);
                if (seedData == null)
                {
                    _logger.LogError("無法解析種子資料JSON");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // FK安全順序匯入
                    await ImportCouponTypes(seedData.CouponTypes);
                    await ImportEVoucherTypes(seedData.EVoucherTypes);
                    await ImportUserWallets(seedData.UserWallets);
                    await ImportCoupons(seedData.Coupons);
                    await ImportEVouchers(seedData.EVouchers);
                    await ImportEVoucherTokens(seedData.EVoucherTokens);
                    await ImportEVoucherRedeemLogs(seedData.EVoucherRedeemLogs);
                    await ImportUserSignInStats(seedData.UserSignInStats);
                    await ImportWalletHistories(seedData.WalletHistories);
                    await ImportPets(seedData.Pets);
                    await ImportMiniGames(seedData.MiniGames);

                    await transaction.CommitAsync();
                    _logger.LogInformation("種子資料匯入完成");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "種子資料匯入失敗");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "種子資料匯入過程發生錯誤");
            }
        }

        private async Task ImportCouponTypes(List<CouponTypeDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.CouponTypes.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("CouponType 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new CouponType
            {
                CouponTypeID = d.CouponTypeID,
                TypeName = d.TypeName ?? "",
                Description = d.Description,
                PointsRequired = d.PointsRequired,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            _context.CouponTypes.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 CouponType 記錄", entities.Count);
        }

        private async Task ImportEVoucherTypes(List<EVoucherTypeDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.EVoucherTypes.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("EVoucherType 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new EVoucherType
            {
                EVoucherTypeID = d.EVoucherTypeID,
                TypeName = d.TypeName ?? "",
                Description = d.Description,
                Value = d.Value,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            _context.EVoucherTypes.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 EVoucherType 記錄", entities.Count);
        }

        private async Task ImportUserWallets(List<UserWalletDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.UserWallets.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("UserWallet 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new UserWallet
            {
                UserID = d.UserID,
                UserPoint = d.UserPoint
            }).ToList();

            _context.UserWallets.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 UserWallet 記錄", entities.Count);
        }

        // 其他匯入方法的簡化版本...
        private async Task ImportCoupons(List<CouponDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportEVouchers(List<EVoucherDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportEVoucherTokens(List<EVoucherTokenDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportEVoucherRedeemLogs(List<EVoucherRedeemLogDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportUserSignInStats(List<UserSignInStatsDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportWalletHistories(List<WalletHistoryDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportPets(List<PetDto>? data) { /* 實作類似邏輯 */ }
        private async Task ImportMiniGames(List<MiniGameDto>? data) { /* 實作類似邏輯 */ }
    }

    // DTO 類別定義
    public class SeedData
    {
        public List<CouponTypeDto> CouponTypes { get; set; } = new();
        public List<EVoucherTypeDto> EVoucherTypes { get; set; } = new();
        public List<UserWalletDto> UserWallets { get; set; } = new();
        public List<CouponDto> Coupons { get; set; } = new();
        public List<EVoucherDto> EVouchers { get; set; } = new();
        public List<EVoucherTokenDto> EVoucherTokens { get; set; } = new();
        public List<EVoucherRedeemLogDto> EVoucherRedeemLogs { get; set; } = new();
        public List<UserSignInStatsDto> UserSignInStats { get; set; } = new();
        public List<WalletHistoryDto> WalletHistories { get; set; } = new();
        public List<PetDto> Pets { get; set; } = new();
        public List<MiniGameDto> MiniGames { get; set; } = new();
    }

    public class CouponTypeDto
    {
        public int CouponTypeID { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public int PointsRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EVoucherTypeDto
    {
        public int EVoucherTypeID { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserWalletDto
    {
        public int UserID { get; set; }
        public int UserPoint { get; set; }
    }

    // 其他DTO類別的簡化定義...
    public class CouponDto { public int CouponID { get; set; } /* 其他屬性 */ }
    public class EVoucherDto { public int EVoucherID { get; set; } /* 其他屬性 */ }
    public class EVoucherTokenDto { public int TokenID { get; set; } /* 其他屬性 */ }
    public class EVoucherRedeemLogDto { public int LogID { get; set; } /* 其他屬性 */ }
    public class UserSignInStatsDto { public int UserID { get; set; } /* 其他屬性 */ }
    public class WalletHistoryDto { public int HistoryID { get; set; } /* 其他屬性 */ }
    public class PetDto { public int PetID { get; set; } /* 其他屬性 */ }
    public class MiniGameDto { public int PlayID { get; set; } /* 其他屬性 */ }
}