using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Services
{
    public interface IGameService
    {
        Task<List<MiniGame>> GetUserGamesAsync(int userId, int page = 1, int pageSize = 20);
        Task<MiniGame?> GetGameByIdAsync(int gameId);
        Task<GameResult> PlayMiniGameAsync(int userId, int petId, string gameType);
        Task<bool> CanPlayGameAsync(int userId);
        Task<int> GetTodayGameCountAsync(int userId);
        Task<List<MiniGame>> GetTopScoresAsync(string gameType, int count = 10);
        Task<GameStats> GetUserGameStatsAsync(int userId);
    }

    public class GameResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty; // Win, Lose, Abort
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string? CouponGained { get; set; }
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
    }

    public class GameStats
    {
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public double WinRate { get; set; }
        public int TodayGames { get; set; }
    }

    public class GameService : IGameService
    {
        private readonly GameSpaceDbContext _context;
        private readonly IWalletService _walletService;
        private readonly IPetService _petService;
        private readonly Random _random = new Random();

        public GameService(GameSpaceDbContext context, IWalletService walletService, IPetService petService)
        {
            _context = context;
            _walletService = walletService;
            _petService = petService;
        }

        public async Task<List<MiniGame>> GetUserGamesAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.MiniGames
                .Where(g => g.UserId == userId)
                .Include(g => g.Pet)
                .OrderByDescending(g => g.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<MiniGame?> GetGameByIdAsync(int gameId)
        {
            return await _context.MiniGames
                .Include(g => g.User)
                .Include(g => g.Pet)
                .FirstOrDefaultAsync(g => g.PlayId == gameId);
        }

        public async Task<GameResult> PlayMiniGameAsync(int userId, int petId, string gameType)
        {
            // 檢查是否可以玩遊戲
            if (!await CanPlayGameAsync(userId))
            {
                return new GameResult
                {
                    Success = false,
                    Message = "今日遊戲次數已達上限！"
                };
            }

            // 檢查寵物是否存在且屬於該用戶
            var pet = await _petService.GetPetByIdAsync(petId);
            if (pet == null || pet.UserId != userId)
            {
                return new GameResult
                {
                    Success = false,
                    Message = "寵物不存在或無權限！"
                };
            }

            // 檢查寵物狀態
            if (pet.Stamina < 20)
            {
                return new GameResult
                {
                    Success = false,
                    Message = "寵物體力不足，需要休息！"
                };
            }

            // 模擬遊戲結果
            var gameResult = SimulateGame(gameType, pet);

            // 創建遊戲記錄
            var miniGame = new MiniGame
            {
                UserId = userId,
                PetId = petId,
                GameType = gameType,
                Result = gameResult.Result,
                PointsGained = gameResult.PointsGained,
                PointsGainedTime = DateTime.UtcNow,
                ExpGained = gameResult.ExpGained,
                ExpGainedTime = DateTime.UtcNow,
                CouponGained = gameResult.CouponGained ?? "0",
                CouponGainedTime = DateTime.UtcNow,
                HungerDelta = gameResult.HungerDelta,
                MoodDelta = gameResult.MoodDelta,
                StaminaDelta = gameResult.StaminaDelta,
                CleanlinessDelta = gameResult.CleanlinessDelta,
                StartTime = DateTime.UtcNow,
                Aborted = gameResult.Result == "Abort"
            };

            _context.MiniGames.Add(miniGame);

            // 更新寵物狀態
            pet.Hunger = Math.Max(0, Math.Min(100, pet.Hunger + gameResult.HungerDelta));
            pet.Mood = Math.Max(0, Math.Min(100, pet.Mood + gameResult.MoodDelta));
            pet.Stamina = Math.Max(0, Math.Min(100, pet.Stamina + gameResult.StaminaDelta));
            pet.Cleanliness = Math.Max(0, Math.Min(100, pet.Cleanliness + gameResult.CleanlinessDelta));
            pet.UpdatedAt = DateTime.UtcNow;

            // 增加用戶點數和經驗
            if (gameResult.PointsGained > 0)
            {
                await _walletService.AddPointsAsync(userId, gameResult.PointsGained, $"小遊戲獎勵 - {gameType}");
            }

            // 更新用戶經驗值
            var user = await _context.Users.FindAsync(userId);
            if (user != null && gameResult.ExpGained > 0)
            {
                user.UserExp += gameResult.ExpGained;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new GameResult
            {
                Success = true,
                Message = gameResult.Result == "Win" ? "遊戲勝利！" : 
                         gameResult.Result == "Lose" ? "遊戲失敗！" : "遊戲中止！",
                Result = gameResult.Result,
                PointsGained = gameResult.PointsGained,
                ExpGained = gameResult.ExpGained,
                CouponGained = gameResult.CouponGained,
                HungerDelta = gameResult.HungerDelta,
                MoodDelta = gameResult.MoodDelta,
                StaminaDelta = gameResult.StaminaDelta,
                CleanlinessDelta = gameResult.CleanlinessDelta
            };
        }

        public async Task<bool> CanPlayGameAsync(int userId)
        {
            var todayGames = await GetTodayGameCountAsync(userId);
            return todayGames < 3; // 每日最多3次
        }

        public async Task<int> GetTodayGameCountAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.MiniGames
                .CountAsync(g => g.UserId == userId && g.StartTime.Date == today);
        }

        public async Task<List<MiniGame>> GetTopScoresAsync(string gameType, int count = 10)
        {
            return await _context.MiniGames
                .Where(g => g.GameType == gameType && g.Result == "Win")
                .OrderByDescending(g => g.PointsGained)
                .Take(count)
                .Include(g => g.User)
                .Include(g => g.Pet)
                .ToListAsync();
        }

        public async Task<GameStats> GetUserGameStatsAsync(int userId)
        {
            var games = await _context.MiniGames
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var todayGames = games.Count(g => g.StartTime.Date == DateTime.UtcNow.Date);
            var wins = games.Count(g => g.Result == "Win");
            var losses = games.Count(g => g.Result == "Lose");
            var totalPoints = games.Sum(g => g.PointsGained);
            var totalExp = games.Sum(g => g.ExpGained);
            var winRate = games.Any() ? (double)wins / games.Count * 100 : 0;

            return new GameStats
            {
                TotalGames = games.Count,
                Wins = wins,
                Losses = losses,
                TotalPoints = totalPoints,
                TotalExp = totalExp,
                WinRate = winRate,
                TodayGames = todayGames
            };
        }

        private GameResult SimulateGame(string gameType, Pet pet)
        {
            // 根據寵物狀態和遊戲類型計算勝率
            var baseWinRate = 0.5;
            var petBonus = (pet.Mood + pet.Stamina) / 200.0; // 寵物狀態加成
            var winRate = Math.Min(0.9, baseWinRate + petBonus);

            var isWin = _random.NextDouble() < winRate;
            var result = isWin ? "Win" : "Lose";

            // 計算獎勵
            var pointsGained = isWin ? _random.Next(10, 50) : _random.Next(1, 10);
            var expGained = isWin ? _random.Next(5, 20) : _random.Next(1, 5);
            string? couponGained = null;

            if (isWin && _random.NextDouble() < 0.1) // 10% 機率獲得優惠券
            {
                couponGained = $"GAME_COUPON_{_random.Next(1000, 9999)}";
            }

            // 計算寵物狀態變化
            var hungerDelta = -_random.Next(5, 15);
            var moodDelta = isWin ? _random.Next(5, 15) : -_random.Next(5, 10);
            var staminaDelta = -_random.Next(10, 25);
            var cleanlinessDelta = -_random.Next(2, 8);

            return new GameResult
            {
                Result = result,
                PointsGained = pointsGained,
                ExpGained = expGained,
                CouponGained = couponGained,
                HungerDelta = hungerDelta,
                MoodDelta = moodDelta,
                StaminaDelta = staminaDelta,
                CleanlinessDelta = cleanlinessDelta
            };
        }
    }
}