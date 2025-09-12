using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class MiniGameController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/MiniGame - 取得小遊戲列表
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var games = await _context.MiniGame
                .Where(g => g.UserID == userId)
                .OrderByDescending(g => g.StartTime)
                .ToListAsync();

            return View(games);
        }

        // POST: MiniGame/MiniGame/Start - 開始新遊戲
        [HttpPost]
        public async Task<IActionResult> Start()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            // 檢查每日遊戲限制（每天 3 場遊戲）
            var todayGames = await _context.MiniGame
                .CountAsync(g => g.UserID == userId && g.StartTime.Date == today);

            if (todayGames >= 3)
            {
                return Json(new { success = false, message = "已達到每日遊戲限制" });
            }

            // 取得使用者的寵物
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            // 檢查寵物健康狀態
            if (pet.Health <= 0)
            {
                return Json(new { success = false, message = "寵物健康狀態不足以遊玩" });
            }

            // 建立新遊戲會話
            var game = new MiniGame
            {
                UserID = userId,
                PetID = pet.PetID,
                Level = 1, // 從第 1 級開始
                MonsterCount = 6,
                SpeedMultiplier = 1.00m,
                Result = "Unknown",
                ExpGained = 0,
                PointsChanged = 0,
                CouponGained = "0",
                HungerDelta = 0,
                MoodDelta = 0,
                StaminaDelta = 0,
                CleanlinessDelta = 0,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                Aborted = false
            };

            _context.MiniGame.Add(game);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "遊戲已開始", gameId = game.PlayID });
        }

        // POST: MiniGame/MiniGame/End - 結束遊戲
        [HttpPost]
        public async Task<IActionResult> End(int gameId, string result)
        {
            var game = await _context.MiniGame
                .FirstOrDefaultAsync(g => g.PlayID == gameId);

            if (game == null)
            {
                return Json(new { success = false, message = "找不到遊戲" });
            }

            // 更新遊戲結果
            game.Result = result;
            game.EndTime = DateTime.UtcNow;

            // 根據結果計算獎勵
            if (result == "Win")
            {
                game.ExpGained = 100;
                game.PointsChanged = 10;
                game.HungerDelta = -20;
                game.MoodDelta = 30;
                game.StaminaDelta = -20;
                game.CleanlinessDelta = -20;
            }
            else if (result == "Lose")
            {
                game.ExpGained = 20;
                game.PointsChanged = 5;
                game.HungerDelta = -20;
                game.MoodDelta = -30;
                game.StaminaDelta = -20;
                game.CleanlinessDelta = -20;
            }

            // 更新寵物屬性
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.PetID == game.PetID);

            if (pet != null)
            {
                pet.Hunger = Math.Max(0, Math.Min(100, pet.Hunger + game.HungerDelta));
                pet.Mood = Math.Max(0, Math.Min(100, pet.Mood + game.MoodDelta));
                pet.Stamina = Math.Max(0, Math.Min(100, pet.Stamina + game.StaminaDelta));
                pet.Cleanliness = Math.Max(0, Math.Min(100, pet.Cleanliness + game.CleanlinessDelta));
                pet.Experience += game.ExpGained;

                // 檢查是否升級
                var requiredExp = CalculateRequiredExp(pet.Level);
                if (pet.Experience >= requiredExp)
                {
                    pet.Level++;
                    pet.LevelUpTime = DateTime.UtcNow;
                    pet.Experience -= requiredExp;
                    pet.PointsGained_levelUp = pet.Level * 10;
                    pet.PointsGainedTime_levelUp = DateTime.UtcNow;
                }
            }

            // 更新使用者錢包
            if (game.PointsChanged > 0)
            {
                var wallet = await _context.User_Wallet
                    .FirstOrDefaultAsync(w => w.User_Id == game.UserID);
                if (wallet != null)
                {
                    wallet.User_Point += game.PointsChanged;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "遊戲已結束", result = result, expGained = game.ExpGained, pointsGained = game.PointsChanged });
        }

        private int CalculateRequiredExp(int level)
        {
            if (level <= 10)
                return 40 * level + 60;
            else if (level <= 100)
                return (int)(0.8 * level * level + 380);
            else
                return (int)(285.69 * Math.Pow(1.06, level));
        }

        public IActionResult StartGame()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StartGame(string gameType, string difficulty)
        {
            var userId = GetCurrentUserId();
            
            var miniGame = new MiniGame
            {
                UserID = userId,
                GameType = gameType,
                StartTime = DateTime.Now,
                Score = 0,
                Point = 0
            };
            
            _context.MiniGame.Add(miniGame);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Play), new { id = miniGame.GameID });
        }

        public IActionResult Play(int id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EndGame(int gameId, int score, int level)
        {
            var miniGame = await _context.MiniGame.FindAsync(gameId);
            if (miniGame == null)
            {
                return NotFound();
            }
            
            miniGame.EndTime = DateTime.Now;
            miniGame.Score = score;
            miniGame.Point = score / 10; // 1 point per 10 score
            
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "遊戲成功結束" });
        }

        private int GetCurrentUserId()
        {
            // TODO: 實作從身份驗證中取得使用者 ID 的適當方法
            return 1; // 暫時的佔位符
        }
    }
}