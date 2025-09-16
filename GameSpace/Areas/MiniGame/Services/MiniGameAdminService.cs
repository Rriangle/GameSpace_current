using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame Area Admin 服務實作
    /// 實作 MiniGame Area 後台管理的業務邏輯
    /// 所有查詢採用 AsNoTracking() 並投影至 ReadModel
    /// </summary>
    public class MiniGameAdminService : IMiniGameAdminService
    {
        private readonly GameSpaceDbContext _context;

        public MiniGameAdminService(GameSpaceDbContext context)
        {
            _context = context;
        }

        #region User_Wallet 模組服務實作

        public async Task<WalletSummaryReadModel> GetWalletSummaryAsync()
        {
            var wallets = await _context.UserWallets.AsNoTracking().ToListAsync();
            
            return new WalletSummaryReadModel
            {
                TotalWallets = wallets.Count,
                TotalPoints = wallets.Sum(w => (long)w.UserPoint),
                AveragePoints = wallets.Any() ? wallets.Average(w => w.UserPoint) : 0,
                VipMembers = wallets.Count(w => w.UserPoint >= 10000)
            };
        }

        public async Task<PagedResult<WalletReadModel>> GetWalletsAsync(WalletQueryModel query)
        {
            var dbQuery = _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(query.Search))
            {
                dbQuery = dbQuery.Where(w => w.User.UserName.Contains(query.Search) || 
                                            w.User.UserNickName.Contains(query.Search));
            }

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .OrderByDescending(w => w.UserPoint)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new WalletReadModel
                {
                    UserId = w.UserID,
                    UserName = w.User.UserName,
                    UserNickName = w.User.UserNickName,
                    UserPoint = w.UserPoint,
                    PointsLevel = GetPointsLevel(w.UserPoint)
                })
                .ToListAsync();

            return new PagedResult<WalletReadModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<WalletDetailReadModel?> GetWalletDetailAsync(int userId)
        {
            var wallet = await _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserID == userId);

            if (wallet == null) return null;

            var recentHistory = await _context.WalletHistories
                .Where(h => h.UserID == userId)
                .OrderByDescending(h => h.ChangeTime)
                .Take(20)
                .AsNoTracking()
                .ToListAsync();

            var totalCoupons = await _context.Coupons
                .AsNoTracking()
                .CountAsync(c => c.UserID == userId);

            var totalEVouchers = await _context.EVouchers
                .AsNoTracking()
                .CountAsync(e => e.UserID == userId);

            return new WalletDetailReadModel
            {
                Wallet = wallet,
                RecentHistory = recentHistory,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers
            };
        }

        #endregion

        #region UserSignInStats 模組服務實作

        public async Task<SignInSummaryReadModel> GetSignInSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.UserSignInStats
                .Where(s => s.SignTime >= startDate && s.SignTime <= endDate.AddDays(1))
                .AsNoTracking();

            var totalSignIns = await query.CountAsync();
            var totalPoints = await query.SumAsync(s => s.PointsChanged);
            var totalExp = await query.SumAsync(s => s.ExpGained);
            var totalCoupons = await query.CountAsync(s => s.CouponGained != "0");
            var uniqueUsers = await query.Select(s => s.UserID).Distinct().CountAsync();

            var dailyStats = await query
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new DailySignInStats
                {
                    Date = g.Key,
                    Count = g.Count(),
                    TotalPoints = g.Sum(s => s.PointsChanged),
                    TotalExp = g.Sum(s => s.ExpGained),
                    CouponsGiven = g.Count(s => s.CouponGained != "0")
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return new SignInSummaryReadModel
            {
                TotalSignIns = totalSignIns,
                TotalPoints = totalPoints,
                TotalExp = totalExp,
                TotalCoupons = totalCoupons,
                UniqueUsers = uniqueUsers,
                DailyStats = dailyStats
            };
        }

        public async Task<PagedResult<SignInStatsReadModel>> GetSignInStatsAsync(SignInQueryModel query)
        {
            var dbQuery = _context.UserSignInStats
                .Include(s => s.User)
                .AsNoTracking();

            // 套用篩選條件
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.SignTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.SignTime <= query.EndDate.Value.AddDays(1));
            }

            if (query.UserId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.UserID == query.UserId.Value);
            }
            else if (!string.IsNullOrEmpty(query.UserSearch))
            {
                dbQuery = dbQuery.Where(s => s.User.UserName.Contains(query.UserSearch) || 
                                            s.User.UserNickName.Contains(query.UserSearch));
            }

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .OrderByDescending(s => s.SignTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new SignInStatsReadModel
                {
                    LogID = s.LogID,
                    UserName = s.User.UserName,
                    UserNickName = s.User.UserNickName,
                    SignTime = s.SignTime,
                    PointsChanged = s.PointsChanged,
                    ExpGained = s.ExpGained,
                    CouponGained = s.CouponGained
                })
                .ToListAsync();

            return new PagedResult<SignInStatsReadModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<UserSignInHistoryReadModel> GetUserSignInHistoryAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return new UserSignInHistoryReadModel();
            }

            var signInHistory = await _context.UserSignInStats
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignTime)
                .AsNoTracking()
                .ToListAsync();

            var signInDates = signInHistory.Select(s => s.SignTime.Date).Distinct().OrderByDescending(d => d).ToList();
            
            return new UserSignInHistoryReadModel
            {
                User = user,
                SignInHistory = signInHistory,
                CurrentStreak = CalculateCurrentStreak(signInDates),
                LongestStreak = CalculateLongestStreak(signInDates),
                TotalSignInDays = signInDates.Count
            };
        }

        #endregion

        #region Pet 模組服務實作

        public async Task<PetSummaryReadModel> GetPetSummaryAsync()
        {
            var pets = await _context.Pets.AsNoTracking().ToListAsync();

            return new PetSummaryReadModel
            {
                TotalPets = pets.Count,
                AverageLevel = pets.Any() ? pets.Average(p => p.Level) : 0,
                MaxLevel = pets.Any() ? pets.Max(p => p.Level) : 0,
                TotalExperience = pets.Sum(p => (long)p.Experience),
                AverageHealth = pets.Any() ? pets.Average(p => p.Health) : 0
            };
        }

        public async Task<PagedResult<PetReadModel>> GetPetsAsync(PetQueryModel query)
        {
            var dbQuery = _context.Pets
                .Include(p => p.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(query.Search))
            {
                dbQuery = dbQuery.Where(p => p.PetName.Contains(query.Search) || 
                                            p.User.UserName.Contains(query.Search) || 
                                            p.User.UserNickName.Contains(query.Search));
            }

            if (query.MinLevel.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Level >= query.MinLevel.Value);
            }

            if (query.MaxLevel.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.Level <= query.MaxLevel.Value);
            }

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PetReadModel
                {
                    PetID = p.PetID,
                    PetName = p.PetName,
                    UserName = p.User.UserName,
                    Level = p.Level,
                    Experience = p.Experience,
                    Health = p.Health,
                    SkinColor = p.SkinColor,
                    BackgroundColor = p.BackgroundColor,
                    LevelUpTime = p.LevelUpTime
                })
                .ToListAsync();

            return new PagedResult<PetReadModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<PetDetailReadModel?> GetPetDetailAsync(int petId)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PetID == petId);

            if (pet == null) return null;

            var recentGames = await _context.MiniGames
                .Where(g => g.PetID == petId)
                .OrderByDescending(g => g.StartTime)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();

            var totalGames = await _context.MiniGames.AsNoTracking().CountAsync(g => g.PetID == petId);
            var winCount = await _context.MiniGames.AsNoTracking().CountAsync(g => g.PetID == petId && g.Result == "Win");

            return new PetDetailReadModel
            {
                Pet = pet,
                RecentGames = recentGames,
                TotalGamesPlayed = totalGames,
                WinRate = totalGames > 0 ? (winCount * 100.0 / totalGames) : 0
            };
        }

        #endregion

        #region MiniGame 模組服務實作

        public async Task<GameSummaryReadModel> GetGameSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.MiniGames
                .Where(m => m.StartTime >= startDate && m.StartTime <= endDate.AddDays(1))
                .AsNoTracking();

            var totalGames = await query.CountAsync();
            var winCount = await query.CountAsync(m => m.Result == "Win");
            var loseCount = await query.CountAsync(m => m.Result == "Lose");
            var abortCount = await query.CountAsync(m => m.Result == "Abort");
            var totalPoints = await query.SumAsync(m => m.PointsChanged);
            var totalExp = await query.SumAsync(m => m.ExpGained);

            var levelStats = await query
                .GroupBy(m => m.Level)
                .Select(g => new LevelStats
                {
                    Level = g.Key,
                    Count = g.Count(),
                    WinRate = g.Count(m => m.Result == "Win") * 100.0 / g.Count(),
                    AvgPoints = g.Average(m => m.PointsChanged),
                    AvgExp = g.Average(m => m.ExpGained)
                })
                .OrderBy(s => s.Level)
                .AsNoTracking()
                .ToListAsync();

            var dailyStats = await query
                .GroupBy(m => m.StartTime.Date)
                .Select(g => new DailyGameStats
                {
                    Date = g.Key,
                    Count = g.Count(),
                    WinCount = g.Count(m => m.Result == "Win"),
                    LoseCount = g.Count(m => m.Result == "Lose"),
                    AbortCount = g.Count(m => m.Result == "Abort")
                })
                .OrderBy(s => s.Date)
                .AsNoTracking()
                .ToListAsync();

            return new GameSummaryReadModel
            {
                TotalGames = totalGames,
                WinCount = winCount,
                LoseCount = loseCount,
                AbortCount = abortCount,
                WinRate = totalGames > 0 ? (winCount * 100.0 / totalGames) : 0,
                TotalPoints = totalPoints,
                TotalExp = totalExp,
                LevelStats = levelStats,
                DailyStats = dailyStats
            };
        }

        public async Task<PagedResult<GameRecordReadModel>> GetGameRecordsAsync(GameQueryModel query)
        {
            var dbQuery = _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .AsNoTracking();

            // 套用篩選條件
            if (!string.IsNullOrEmpty(query.Result))
            {
                dbQuery = dbQuery.Where(m => m.Result == query.Result);
            }

            if (query.Level.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.Level == query.Level.Value);
            }

            if (query.UserId.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.UserID == query.UserId.Value);
            }

            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.StartTime <= query.EndDate.Value.AddDays(1));
            }

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .OrderByDescending(m => m.StartTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(m => new GameRecordReadModel
                {
                    PlayID = m.PlayID,
                    UserName = m.User.UserName,
                    PetName = m.Pet.PetName,
                    Level = m.Level,
                    Result = m.Result,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    PointsChanged = m.PointsChanged,
                    ExpGained = m.ExpGained,
                    Aborted = m.Aborted
                })
                .ToListAsync();

            return new PagedResult<GameRecordReadModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<GameDetailReadModel?> GetGameDetailAsync(int playId)
        {
            var game = await _context.MiniGames
                .Include(m => m.User)
                .Include(m => m.Pet)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PlayID == playId);

            if (game == null) return null;

            var duration = game.EndTime.HasValue ? game.EndTime.Value - game.StartTime : null;
            var difficultyLevel = GetDifficultyLevel(game.Level, game.MonsterCount, game.SpeedMultiplier);

            return new GameDetailReadModel
            {
                Game = game,
                Pet = game.Pet,
                User = game.User,
                Duration = duration,
                DifficultyLevel = difficultyLevel
            };
        }

        #endregion

        #region Helper Methods

        private static string GetPointsLevel(int points)
        {
            if (points >= 10000) return "VIP會員";
            if (points >= 5000) return "金牌會員";
            if (points >= 1000) return "銀牌會員";
            return "一般會員";
        }

        private static int CalculateCurrentStreak(List<DateTime> signInDates)
        {
            if (!signInDates.Any()) return 0;

            var today = DateTime.Today;
            var streak = 0;
            var checkDate = signInDates.Contains(today) ? today : today.AddDays(-1);

            foreach (var date in signInDates.OrderByDescending(d => d))
            {
                if (date == checkDate)
                {
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private static int CalculateLongestStreak(List<DateTime> signInDates)
        {
            if (!signInDates.Any()) return 0;

            var sortedDates = signInDates.OrderBy(d => d).ToList();
            var longestStreak = 1;
            var currentStreak = 1;

            for (int i = 1; i < sortedDates.Count; i++)
            {
                if (sortedDates[i] == sortedDates[i - 1].AddDays(1))
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }

        private static string GetDifficultyLevel(int level, int monsterCount, decimal speedMultiplier)
        {
            var difficultyScore = level * monsterCount * (double)speedMultiplier;
            
            if (difficultyScore >= 30) return "極困難";
            if (difficultyScore >= 20) return "困難";
            if (difficultyScore >= 10) return "普通";
            return "簡單";
        }

        #endregion
    }
}