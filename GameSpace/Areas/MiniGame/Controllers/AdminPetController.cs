using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - Pet 模組後台管理控制器
    /// 負責管理寵物系統的後台功能
    /// 資料表範圍：Pet（以 database.json 為準）
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    [MiniGameAdminAuthorize]
    public class AdminPetController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminPetController(GameSpaceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 寵物列表頁面 - Read-first 原則
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, 
            string search = "", int? minLevel = null, int? maxLevel = null)
        {
            var query = _context.Pets
                .Include(p => p.User)
                .AsNoTracking();

            // 搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.PetName.Contains(search) || 
                                        p.User.UserName.Contains(search) || 
                                        p.User.UserNickName.Contains(search));
            }

            if (minLevel.HasValue)
            {
                query = query.Where(p => p.Level >= minLevel.Value);
            }

            if (maxLevel.HasValue)
            {
                query = query.Where(p => p.Level <= maxLevel.Value);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var pets = await query
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.MinLevel = minLevel;
            ViewBag.MaxLevel = maxLevel;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(pets);
        }

        /// <summary>
        /// 寵物明細頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PetID == id);

            if (pet == null)
            {
                return NotFound("找不到指定的寵物");
            }

            return View(pet);
        }

        /// <summary>
        /// 寵物統計頁面
        /// </summary>
        public async Task<IActionResult> Statistics()
        {
            // 統計資料查詢 - Read-first
            var totalPets = await _context.Pets.AsNoTracking().CountAsync();
            var averageLevel = await _context.Pets.AsNoTracking().AverageAsync(p => (double)p.Level);
            var maxLevel = await _context.Pets.AsNoTracking().MaxAsync(p => p.Level);
            var totalExperience = await _context.Pets.AsNoTracking().SumAsync(p => p.Experience);

            // 等級分佈統計
            var levelDistribution = await _context.Pets
                .GroupBy(p => p.Level / 10 * 10) // 以10級為區間分組
                .Select(g => new { LevelRange = g.Key, Count = g.Count() })
                .OrderBy(g => g.LevelRange)
                .AsNoTracking()
                .ToListAsync();

            // 屬性統計
            var attributeStats = await _context.Pets
                .Select(p => new {
                    AvgHunger = p.Hunger,
                    AvgMood = p.Mood,
                    AvgStamina = p.Stamina,
                    AvgCleanliness = p.Cleanliness,
                    AvgHealth = p.Health
                })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.TotalPets = totalPets;
            ViewBag.AverageLevel = averageLevel;
            ViewBag.MaxLevel = maxLevel;
            ViewBag.TotalExperience = totalExperience;
            ViewBag.LevelDistribution = levelDistribution;
            ViewBag.AttributeStats = attributeStats;

            return View();
        }

        /// <summary>
        /// 寵物狀態調整功能 - 預留實作介面
        /// 根據指令：寫入流程僅可建立不破壞規格的預留實作
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStatus(int petId, string adjustmentType, int value, string reason)
        {
            // 預留實作：驗證與流程說明到位，實際寫入前先經過後續階段與規格允許再開啟
            
            var pet = await _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PetID == petId);

            if (pet == null)
            {
                TempData["ErrorMessage"] = "找不到指定的寵物";
                return RedirectToAction(nameof(Index));
            }

            // 驗證調整類型
            var validTypes = new[] { "Level", "Experience", "Hunger", "Mood", "Stamina", "Cleanliness", "Health" };
            if (!validTypes.Contains(adjustmentType))
            {
                TempData["ErrorMessage"] = "無效的調整類型";
                return RedirectToAction(nameof(Details), new { id = petId });
            }

            // 驗證調整值範圍
            if (adjustmentType != "Level" && adjustmentType != "Experience")
            {
                if (value < 0 || value > 100)
                {
                    TempData["ErrorMessage"] = "屬性值必須在 0-100 範圍內";
                    return RedirectToAction(nameof(Details), new { id = petId });
                }
            }

            // 驗證原因說明
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "請提供調整原因說明";
                return RedirectToAction(nameof(Details), new { id = petId });
            }

            // 預留實作提示：實際寫入功能待後續階段開啟
            TempData["InfoMessage"] = $"寵物狀態調整功能預留實作中。" +
                $"預計調整：{pet.PetName} 的 {adjustmentType} 為 {value}，原因：{reason}";

            return RedirectToAction(nameof(Details), new { id = petId });
        }

        /// <summary>
        /// 批次寵物維護功能
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchMaintenance(string maintenanceType)
        {
            // 預留實作：提供批次維護的驗證與流程說明
            
            var validTypes = new[] { "DailyDecay", "LevelUpReward", "AttributeReset", "ExperienceBonus" };
            if (!validTypes.Contains(maintenanceType))
            {
                TempData["ErrorMessage"] = "無效的維護類型";
                return RedirectToAction(nameof(Index));
            }

            var affectedCount = await _context.Pets.AsNoTracking().CountAsync();

            // 預留實作提示：實際批次處理功能待後續階段開啟
            TempData["InfoMessage"] = $"批次維護功能預留實作中。預計影響 {affectedCount} 隻寵物，維護類型：{maintenanceType}";

            return RedirectToAction(nameof(Index));
        }

        #region 寵物規則和編輯功能

        /// <summary>
        /// 寵物系統規則預覽頁面（唯讀）
        /// </summary>
        public IActionResult RulePreview()
        {
            // 從配置或程式常數讀取寵物規則
            var petRules = new
            {
                // 等級系統
                MaxLevel = 100,                    // 最高等級
                ExpPerLevel = 1000,                // 每級所需經驗值
                LevelUpReward = 50,                // 升級獎勵點數
                
                // 屬性範圍
                AttributeMin = 0,                  // 屬性最小值
                AttributeMax = 100,                // 屬性最大值
                HealthCritical = 20,               // 健康值危險線
                
                // 互動獲得
                FeedGain = 10,                     // 餵食獲得
                PlayGain = 15,                     // 遊玩獲得
                CleanGain = 8,                     // 清潔獲得
                RestGain = 12,                     // 休息獲得
                
                // 顏色變更成本
                SkinColorCost = 100,               // 皮膚顏色變更點數
                BackgroundColorCost = 150,         // 背景顏色變更點數
                
                // 可用顏色
                AvailableSkinColors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F" },
                AvailableBackgroundColors = new[] { "#2C3E50", "#34495E", "#7F8C8D", "#95A5A6", "#BDC3C7", "#ECF0F1", "#E8F8F5", "#FDF2E9" },
                
                // 系統設定
                DecayRate = 5,                     // 每日屬性衰減
                AutoSaveInterval = 300,            // 自動儲存間隔（秒）
                
                // 配置資訊
                ConfigSource = "程式常數",         // 配置來源
                LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Version = "2.1.0"                  // 規則版本
            };

            ViewBag.PetRules = petRules;
            return View();
        }

        /// <summary>
        /// 顯示寵物編輯表單
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PetID == id);

            if (pet == null)
            {
                return NotFound("找不到指定的寵物");
            }

            ViewBag.Pet = pet;
            return View();
        }

        /// <summary>
        /// 執行寵物編輯 - 交易性操作
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string petName, int level, int experience, 
            int hunger, int mood, int stamina, int cleanliness, int health, 
            string skinColor, string backgroundColor, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "編輯原因為必填";
                return RedirectToAction(nameof(Edit), new { id });
            }

            // 驗證屬性範圍
            if (level < 1 || level > 100 || experience < 0 || 
                hunger < 0 || hunger > 100 || mood < 0 || mood > 100 ||
                stamina < 0 || stamina > 100 || cleanliness < 0 || cleanliness > 100 ||
                health < 0 || health > 100)
            {
                TempData["Error"] = "屬性值超出有效範圍（等級：1-100，其他屬性：0-100）";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetID == id);

                if (pet == null)
                {
                    TempData["Error"] = "找不到指定的寵物";
                    return RedirectToAction(nameof(Index));
                }

                // 記錄變更前的狀態
                var oldValues = new
                {
                    PetName = pet.PetName,
                    Level = pet.Level,
                    Experience = pet.Experience,
                    Hunger = pet.Hunger,
                    Mood = pet.Mood,
                    Stamina = pet.Stamina,
                    Cleanliness = pet.Cleanliness,
                    Health = pet.Health,
                    SkinColor = pet.SkinColor,
                    BackgroundColor = pet.BackgroundColor
                };

                // 更新寵物屬性
                pet.PetName = petName?.Trim() ?? pet.PetName;
                pet.Level = level;
                pet.Experience = experience;
                pet.Hunger = hunger;
                pet.Mood = mood;
                pet.Stamina = stamina;
                pet.Cleanliness = cleanliness;
                pet.Health = health;
                pet.SkinColor = skinColor ?? pet.SkinColor;
                pet.BackgroundColor = backgroundColor ?? pet.BackgroundColor;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄日誌
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminPetController>>();
                logger.LogInformation("管理員編輯寵物: PetId={PetId}, UserId={UserId}, Changes={Changes}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    id, pet.UserID, System.Text.Json.JsonSerializer.Serialize(new { Old = oldValues, New = new { petName, level, experience, hunger, mood, stamina, cleanliness, health, skinColor, backgroundColor } }), 
                    reason, adminUserId, traceId);

                TempData["Success"] = $"成功更新寵物 {pet.PetName} 的資料";
                return RedirectToAction(nameof(Details), new { id });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminPetController>>();
                logger.LogError(ex, "寵物編輯失敗: PetId={PetId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    id, reason, adminUserId, traceId);

                TempData["Error"] = "寵物編輯失敗：" + ex.Message;
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        /// <summary>
        /// 寵物顏色變更歷史
        /// </summary>
        public async Task<IActionResult> ColorHistory(int userId)
        {
            // 載入會員資訊
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.User_ID == userId);

            if (user == null)
            {
                return NotFound("找不到指定的會員");
            }

            // 載入會員寵物
            var userPet = await _context.Pets
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserID == userId);

            // 查詢顏色變更相關的錢包歷史記錄
            var colorHistory = await _context.WalletHistories
                .Where(h => h.UserID == userId && 
                           (h.ItemCode == "PetColorChange" || 
                            h.ItemCode == "PetBgColorChange" || 
                            h.ItemCode == "WalletAdjust"))
                .AsNoTracking()
                .OrderByDescending(h => h.ChangeTime)
                .Take(50) // 最近50筆記錄
                .AsNoTracking()
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserPet = userPet;
            ViewBag.ColorHistory = colorHistory;

            return View();
        }

        #endregion

        #region 匯出功能

        /// <summary>
        /// 寵物列表 CSV 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportCsv(string search = "", int? minLevel = null, int? maxLevel = null)
        {
            var query = BuildPetQuery(search, minLevel, maxLevel);
            
            var pets = await query
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .AsNoTracking()
                .ToListAsync();

            var csvData = pets.Select(p => new
            {
                寵物ID = p.PetID,
                寵物名稱 = p.PetName,
                擁有者ID = p.UserID,
                擁有者名稱 = p.User?.UserNickName ?? p.User?.UserName ?? "未知",
                等級 = p.Level,
                經驗值 = p.Experience,
                飢餓值 = p.Hunger,
                心情值 = p.Mood,
                體力值 = p.Stamina,
                清潔值 = p.Cleanliness,
                健康值 = p.Health,
                皮膚顏色 = p.SkinColor,
                背景顏色 = p.BackgroundColor,
                最後升級時間 = p.LevelUpTime.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return ExportService.CreateJsonFile(
                ExportService.CreateExportData(csvData, new {
                    篩選條件 = new { 
                        搜尋關鍵字 = search ?? "無", 
                        最低等級 = minLevel?.ToString() ?? "不限",
                        最高等級 = maxLevel?.ToString() ?? "不限"
                    }
                }),
                "pets"
            );
        }

        /// <summary>
        /// 寵物列表 JSON 匯出
        /// </summary>
        public async Task<IActionResult> IndexExportJson(string search = "", int? minLevel = null, int? maxLevel = null)
        {
            return await IndexExportCsv(search, minLevel, maxLevel);
        }

        /// <summary>
        /// 建立寵物查詢（可重用）
        /// </summary>
        private IQueryable<Pet> BuildPetQuery(string search, int? minLevel, int? maxLevel)
        {
            var query = _context.Pets
                .Include(p => p.User)
                .AsNoTracking();

            // 搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.PetName.Contains(search) || 
                                        p.User.UserName.Contains(search) || 
                                        p.User.UserNickName.Contains(search));
            }

            if (minLevel.HasValue)
            {
                query = query.Where(p => p.Level >= minLevel.Value);
            }

            if (maxLevel.HasValue)
            {
                query = query.Where(p => p.Level <= maxLevel.Value);
            }

            return query;
        }

        #endregion
    }
}