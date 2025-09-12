using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                // Create default pet if it doesn't exist
                pet = new Pet
                {
                    UserID = userId,
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
                    ColorChangedTime = DateTime.UtcNow,
                    BackgroundColor = "粉藍",
                    BackgroundColorChangedTime = DateTime.UtcNow,
                    PointsChanged_color = 0,
                    PointsChangedTime_color = DateTime.UtcNow,
                    PointsGained_levelUp = 0,
                    PointsGainedTime_levelUp = DateTime.UtcNow
                };
                _context.Pet.Add(pet);
                await _context.SaveChangesAsync();
            }

            return View(pet);
        }

        // POST: MiniGame/Pet/Feed
        [HttpPost]
        public async Task<IActionResult> Feed()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "Pet not found" });
            }

            // Feed pet - increase hunger and mood
            pet.Hunger = Math.Min(100, pet.Hunger + 10);
            pet.Mood = Math.Min(100, pet.Mood + 10);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Pet fed successfully", hunger = pet.Hunger, mood = pet.Mood });
        }

        // POST: MiniGame/Pet/Play
        [HttpPost]
        public async Task<IActionResult> Play()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "Pet not found" });
            }

            // Play with pet - increase mood but decrease stamina
            pet.Mood = Math.Min(100, pet.Mood + 10);
            pet.Stamina = Math.Max(0, pet.Stamina - 5);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Played with pet successfully", mood = pet.Mood, stamina = pet.Stamina });
        }

        // POST: MiniGame/Pet/Clean
        [HttpPost]
        public async Task<IActionResult> Clean()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "Pet not found" });
            }

            // Clean pet - increase cleanliness
            pet.Cleanliness = Math.Min(100, pet.Cleanliness + 10);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Pet cleaned successfully", cleanliness = pet.Cleanliness });
        }

        // POST: MiniGame/Pet/Rest
        [HttpPost]
        public async Task<IActionResult> Rest()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pet
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "Pet not found" });
            }

            // Rest pet - increase stamina
            pet.Stamina = Math.Min(100, pet.Stamina + 10);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Pet rested successfully", stamina = pet.Stamina });
        }

        private int GetCurrentUserId()
        {
            // TODO: Implement proper user ID retrieval from authentication
            return 1; // Placeholder
        }
    }
}