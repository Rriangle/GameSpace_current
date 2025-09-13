using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class PetController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public PetController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/Pet
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                // 如果寵物不存在則建立預設寵物
                pet = new Pet
                {
                    UserId = userId,
                    PetName = "小可愛",
                    Level = 0,
                    LevelUpTime = DateTime.UtcNow,
                    Experience = 0,
                    Hunger = 0,
                    Mood = 0,
                    Stamina = 0,
                    Cleanliness = 0,
                    Health = 100,
                    SkinColor = "#ADD8E6",
                    SkinColorChangedTime = DateTime.UtcNow,
                    BackgroundColor = "粉藍",
                    BackgroundColorChangedTime = DateTime.UtcNow,
                    PointsChangedSkinColor = 0,
                    PointsChangedBackgroundColor = 0,
                    PointsGainedLevelUp = 0,
                    PointsGainedTimeLevelUp = DateTime.UtcNow
                };
                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
            }

            return View(pet);
        }

        // POST: MiniGame/Pet/Feed - 餵食
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feed()
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 餵食 - 增加飢餓值（降低飢餓感）和心情值
                pet.Hunger = Math.Max(0, pet.Hunger - 10); // 飢餓值降低表示不餓
                pet.Mood = Math.Min(100, pet.Mood + 10);
                
                // 檢查是否所有屬性都滿了，給予額外經驗
                if (pet.Hunger == 0 && pet.Mood == 100 && pet.Stamina == 100 && pet.Cleanliness == 100)
                {
                    pet.Experience += 100; // 每日狀態全滿獎勵
                    await CheckLevelUp(pet, userId);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "成功餵食寵物！", 
                    hunger = pet.Hunger, 
                    mood = pet.Mood,
                    experience = pet.Experience,
                    level = pet.Level
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"餵食失敗：{ex.Message}" });
            }
        }

        // POST: MiniGame/Pet/Play - 陪玩
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play()
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 陪玩 - 增加心情但減少體力
                pet.Mood = Math.Min(100, pet.Mood + 10);
                pet.Stamina = Math.Max(0, pet.Stamina - 5);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "和寵物玩得很開心！", 
                    mood = pet.Mood, 
                    stamina = pet.Stamina 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"陪玩失敗：{ex.Message}" });
            }
        }

        // POST: MiniGame/Pet/Clean - 洗澡
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clean()
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 洗澡 - 增加清潔值
                pet.Cleanliness = Math.Min(100, pet.Cleanliness + 10);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "寵物洗得乾乾淨淨！", 
                    cleanliness = pet.Cleanliness 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"洗澡失敗：{ex.Message}" });
            }
        }

        // POST: MiniGame/Pet/Rest - 哄睡
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rest()
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 哄睡 - 增加體力
                pet.Stamina = Math.Min(100, pet.Stamina + 10);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "寵物睡得很香甜！", 
                    stamina = pet.Stamina 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"哄睡失敗：{ex.Message}" });
            }
        }

        // POST: MiniGame/Pet/ChangeSkinColor - 換膚色
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeSkinColor(string newColor)
        {
            if (string.IsNullOrEmpty(newColor))
            {
                return Json(new { success = false, message = "請選擇顏色" });
            }

            var userId = GetCurrentUserId();
            const int skinColorCost = 2000;
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                if (wallet == null || wallet.UserPoint < skinColorCost)
                {
                    return Json(new { success = false, message = "點數不足，需要2000點" });
                }

                // 扣除點數
                wallet.UserPoint -= skinColorCost;
                
                // 更換膚色
                pet.SkinColor = newColor;
                pet.SkinColorChangedTime = DateTime.UtcNow;
                pet.PointsChangedSkinColor = skinColorCost;

                // 記錄錢包異動
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "SkinColor",
                    PointsChanged = -skinColorCost,
                    Description = $"購買寵物膚色：{newColor}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "成功更換寵物膚色！", 
                    newColor = newColor,
                    remainingPoints = wallet.UserPoint
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"更換膚色失敗：{ex.Message}" });
            }
        }

        // 檢查寵物是否升級並給予獎勵
        private async Task CheckLevelUp(Pet pet, int userId)
        {
            // 計算升級所需經驗
            int requiredExp = CalculateRequiredExp(pet.Level);
            
            if (pet.Experience >= requiredExp)
            {
                // 升級
                pet.Level++;
                pet.Experience -= requiredExp; // 扣除升級所需經驗
                pet.LevelUpTime = DateTime.UtcNow;
                
                // 計算點數獎勵 (等級1-10每升級+10點，11-20每升級+20點，以此類推)
                int pointsReward = Math.Min(250, (pet.Level / 10 + 1) * 10);
                pet.PointsGainedLevelUp = pointsReward;
                pet.PointsGainedTimeLevelUp = DateTime.UtcNow;
                
                // 給予主人點數獎勵
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.UserPoint += pointsReward;
                }

                // 記錄錢包異動
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "PetLevelUp",
                    PointsChanged = pointsReward,
                    Description = $"寵物升級獎勵（Lv.{pet.Level}）",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);
                
                // 升級時恢復所有屬性到100
                pet.Hunger = 0;   // 飢餓值0表示不餓
                pet.Mood = 100;
                pet.Stamina = 100;
                pet.Cleanliness = 100;
                pet.Health = 100;
            }
        }

        // 計算升級所需經驗
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

        private int GetCurrentUserId()
        {
            // 暫時使用固定用戶ID進行測試
            // TODO: 實作適當的用戶身份驗證
            return 1;
        }
    }
}