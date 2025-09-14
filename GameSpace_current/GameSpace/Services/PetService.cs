using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameSpace.Services
{
    public interface IPetService
    {
        Task<List<Pet>> GetPetsByUserIdAsync(int userId);
        Task<Pet?> GetPetByIdAsync(int petId);
        Task<Pet> CreatePetAsync(Pet pet);
        Task<Pet> UpdatePetAsync(Pet pet);
        Task<bool> DeletePetAsync(int petId);
        Task<bool> FeedPetAsync(int petId, int hungerIncrease);
        Task<bool> PlayWithPetAsync(int petId, int moodIncrease);
        Task<bool> CleanPetAsync(int petId, int cleanlinessIncrease);
        Task<bool> RestPetAsync(int petId, int staminaIncrease);
        Task<Pet?> GetUserActivePetAsync(int userId);
        Task<bool> SetActivePetAsync(int userId, int petId);
    }

    public class PetService : IPetService
    {
        private readonly GameSpaceDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PetService> _logger;

        public PetService(GameSpaceDbContext context, ICacheService cacheService, IConfiguration configuration, ILogger<PetService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<Pet>> GetPetsByUserIdAsync(int userId)
        {
            var cacheKey = $"pets_user_{userId}";
            var cacheExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:UserProfileExpirationMinutes", 60));

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("從資料庫載入寵物資料: UserId={UserId}", userId);
                
                return await _context.Pets
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }, cacheExpiration);
        }

        public async Task<Pet?> GetPetByIdAsync(int petId)
        {
            var cacheKey = $"pet_{petId}";
            var cacheExpiration = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:UserProfileExpirationMinutes", 60));

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("從資料庫載入寵物資料: PetId={PetId}", petId);
                
                return await _context.Pets
                    .Include(p => p.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PetId == petId);
            }, cacheExpiration);
        }

        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            pet.CreatedAt = DateTime.UtcNow;
            pet.UpdatedAt = DateTime.UtcNow;
            
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            
            // 清除相關快取
            await _cacheService.RemoveAsync($"pets_user_{pet.UserId}");
            
            _logger.LogInformation("建立新寵物: PetId={PetId}, UserId={UserId}", pet.PetId, pet.UserId);
            
            return pet;
        }

        public async Task<Pet> UpdatePetAsync(Pet pet)
        {
            pet.UpdatedAt = DateTime.UtcNow;
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
            
            // 清除相關快取
            await _cacheService.RemoveAsync($"pet_{pet.PetId}");
            await _cacheService.RemoveAsync($"pets_user_{pet.UserId}");
            
            _logger.LogInformation("更新寵物資料: PetId={PetId}, UserId={UserId}", pet.PetId, pet.UserId);
            
            return pet;
        }

        public async Task<bool> DeletePetAsync(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FeedPetAsync(int petId, int hungerIncrease)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;

            pet.Hunger = Math.Min(100, pet.Hunger + hungerIncrease);
            pet.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PlayWithPetAsync(int petId, int moodIncrease)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;

            pet.Mood = Math.Min(100, pet.Mood + moodIncrease);
            pet.Stamina = Math.Max(0, pet.Stamina - 10); // 玩耍消耗體力
            pet.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CleanPetAsync(int petId, int cleanlinessIncrease)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;

            pet.Cleanliness = Math.Min(100, pet.Cleanliness + cleanlinessIncrease);
            pet.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestPetAsync(int petId, int staminaIncrease)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return false;

            pet.Stamina = Math.Min(100, pet.Stamina + staminaIncrease);
            pet.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pet?> GetUserActivePetAsync(int userId)
        {
            return await _context.Pets
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetActivePetAsync(int userId, int petId)
        {
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == petId && p.UserId == userId);
            
            if (pet == null) return false;

            // 更新寵物的最後活動時間
            pet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}