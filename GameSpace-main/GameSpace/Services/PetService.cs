using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

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

        public PetService(GameSpaceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pet>> GetPetsByUserIdAsync(int userId)
        {
            return await _context.Pets
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Pet?> GetPetByIdAsync(int petId)
        {
            return await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            pet.CreatedAt = DateTime.UtcNow;
            pet.UpdatedAt = DateTime.UtcNow;
            
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        public async Task<Pet> UpdatePetAsync(Pet pet)
        {
            pet.UpdatedAt = DateTime.UtcNow;
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
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