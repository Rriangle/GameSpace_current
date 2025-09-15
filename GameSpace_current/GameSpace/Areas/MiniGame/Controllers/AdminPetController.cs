using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - Pet 模組後台管理控制器
    /// 負責管理寵物系統的後台功能
    /// 資料表範圍：Pet（以 database.json 為準）
    /// 根據指令第[4]節：其餘表以後台管理的查詢與審閱頁為主，寫入功能為預留實作
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
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

            var totalCount = await query.CountAsync();
            var pets = await query
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
            var totalPets = await _context.Pets.CountAsync();
            var averageLevel = await _context.Pets.AverageAsync(p => (double)p.Level);
            var maxLevel = await _context.Pets.MaxAsync(p => p.Level);
            var totalExperience = await _context.Pets.SumAsync(p => p.Experience);

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

            var affectedCount = await _context.Pets.CountAsync();

            // 預留實作提示：實際批次處理功能待後續階段開啟
            TempData["InfoMessage"] = $"批次維護功能預留實作中。預計影響 {affectedCount} 隻寵物，維護類型：{maintenanceType}";

            return RedirectToAction(nameof(Index));
        }
    }
}