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

        private async Task ImportCoupons(List<CouponDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.Coupons.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("Coupon 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new Coupon
            {
                CouponID = d.CouponID,
                CouponCode = d.CouponCode ?? "",
                CouponTypeID = d.CouponTypeID,
                UserID = d.UserID,
                IsUsed = d.IsUsed,
                AcquiredTime = d.AcquiredTime,
                UsedTime = d.UsedTime
            }).ToList();

            _context.Coupons.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 Coupon 記錄", entities.Count);
        }

        private async Task ImportEVouchers(List<EVoucherDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.EVouchers.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("EVoucher 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new EVoucher
            {
                EVoucherID = d.EVoucherID,
                EVoucherCode = d.EVoucherCode ?? "",
                EVoucherTypeID = d.EVoucherTypeID,
                UserID = d.UserID,
                IsUsed = d.IsUsed,
                AcquiredTime = d.AcquiredTime,
                UsedTime = d.UsedTime
            }).ToList();

            _context.EVouchers.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 EVoucher 記錄", entities.Count);
        }

        private async Task ImportEVoucherTokens(List<EVoucherTokenDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.EVoucherTokens.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("EVoucherToken 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new EVoucherToken
            {
                TokenID = d.TokenID,
                EVoucherID = d.EVoucherID,
                Token = d.Token ?? "",
                IsRevoked = d.IsRevoked,
                ExpiresAt = d.ExpiresAt
            }).ToList();

            _context.EVoucherTokens.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 EVoucherToken 記錄", entities.Count);
        }

        private async Task ImportEVoucherRedeemLogs(List<EVoucherRedeemLogDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.EVoucherRedeemLogs.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("EVoucherRedeemLog 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new EVoucherRedeemLog
            {
                RedeemLogID = d.RedeemLogID,
                EVoucherID = d.EVoucherID,
                TokenID = d.TokenID,
                UserID = d.UserID,
                ScannedAt = d.ScannedAt,
                Status = d.Status ?? ""
            }).ToList();

            _context.EVoucherRedeemLogs.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 EVoucherRedeemLog 記錄", entities.Count);
        }

        private async Task ImportUserSignInStats(List<UserSignInStatsDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.UserSignInStats.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("UserSignInStats 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new UserSignInStats
            {
                UserID = d.UserID,
                ConsecutiveDays = d.ConsecutiveDays,
                TotalSignIns = d.TotalSignIns,
                LastSignInDate = d.LastSignInDate
            }).ToList();

            _context.UserSignInStats.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 UserSignInStats 記錄", entities.Count);
        }

        private async Task ImportWalletHistories(List<WalletHistoryDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.WalletHistories.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("WalletHistory 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new WalletHistory
            {
                UserID = d.UserID,
                ChangeType = d.ChangeType ?? "",
                PointsChanged = d.PointsChanged,
                ItemCode = d.ItemCode,
                Description = d.Description ?? "",
                ChangeTime = d.ChangeTime
            }).ToList();

            _context.WalletHistories.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 WalletHistory 記錄", entities.Count);
        }

        private async Task ImportPets(List<PetDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.Pets.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("Pet 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new Pet
            {
                UserID = d.UserID,
                PetName = d.PetName ?? "",
                Level = d.Level,
                Experience = d.Experience,
                Health = d.Health,
                Hunger = d.Hunger,
                Happiness = d.Happiness,
                Energy = d.Energy,
                Cleanliness = d.Cleanliness,
                SkinColor = d.SkinColor ?? "#FFFFFF",
                BackgroundTheme = d.BackgroundTheme ?? "default"
            }).ToList();

            _context.Pets.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 Pet 記錄", entities.Count);
        }

        private async Task ImportMiniGames(List<MiniGameDto>? data)
        {
            if (data?.Any() != true) return;

            var hasData = await _context.MiniGames.AsNoTracking().AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("MiniGame 資料表已有資料，跳過匯入");
                return;
            }

            var entities = data.Select(d => new MiniGame
            {
                UserID = d.UserID,
                PetID = d.PetID,
                Level = d.Level,
                MonsterCount = d.MonsterCount,
                SpeedMultiplier = d.SpeedMultiplier,
                Result = d.Result ?? "",
                ExpGained = d.ExpGained,
                PointsGained = d.PointsGained,
                StartTime = d.StartTime,
                EndTime = d.EndTime
            }).ToList();

            _context.MiniGames.AddRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("匯入 {Count} 筆 MiniGame 記錄", entities.Count);
        }
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

    public class CouponDto
    {
        public int CouponID { get; set; }
        public string? CouponCode { get; set; }
        public int CouponTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }

    public class EVoucherDto
    {
        public int EVoucherID { get; set; }
        public string? EVoucherCode { get; set; }
        public int EVoucherTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
    }

    public class EVoucherTokenDto
    {
        public int TokenID { get; set; }
        public int EVoucherID { get; set; }
        public string? Token { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class EVoucherRedeemLogDto
    {
        public int RedeemLogID { get; set; }
        public int EVoucherID { get; set; }
        public int? TokenID { get; set; }
        public int UserID { get; set; }
        public DateTime ScannedAt { get; set; }
        public string? Status { get; set; }
    }

    public class UserSignInStatsDto
    {
        public int UserID { get; set; }
        public int ConsecutiveDays { get; set; }
        public int TotalSignIns { get; set; }
        public DateTime? LastSignInDate { get; set; }
    }

    public class WalletHistoryDto
    {
        public int HistoryID { get; set; }
        public int UserID { get; set; }
        public string? ChangeType { get; set; }
        public int PointsChanged { get; set; }
        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public DateTime ChangeTime { get; set; }
    }

    public class PetDto
    {
        public int PetID { get; set; }
        public int UserID { get; set; }
        public string? PetName { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Happiness { get; set; }
        public int Energy { get; set; }
        public int Cleanliness { get; set; }
        public string? SkinColor { get; set; }
        public string? BackgroundTheme { get; set; }
    }

    public class MiniGameDto
    {
        public int PlayID { get; set; }
        public int UserID { get; set; }
        public int PetID { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string? Result { get; set; }
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}