using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Services
{
    public interface IForumService
    {
        Task<List<Forum>> GetForumsAsync(int page = 1, int pageSize = 20, string? category = null);
        Task<Forum?> GetForumByIdAsync(int forumId);
        Task<Forum> CreateForumAsync(Forum forum);
        Task<Forum> UpdateForumAsync(Forum forum);
        Task<bool> DeleteForumAsync(int forumId);
        Task<List<Forum>> SearchForumsAsync(string keyword, int page = 1, int pageSize = 20);
        Task<List<Forum>> GetUserForumsAsync(int userId, int page = 1, int pageSize = 20);
        Task<List<Forum>> GetPopularForumsAsync(int count = 10);
        Task<ForumStats> GetForumStatsAsync(int forumId);
    }

    public class ForumStats
    {
        public int ViewCount { get; set; }
        public int ReplyCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class ForumService : IForumService
    {
        private readonly GameSpaceDbContext _context;

        public ForumService(GameSpaceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Forum>> GetForumsAsync(int page = 1, int pageSize = 20, string? category = null)
        {
            var query = _context.Forums
                .Include(f => f.User)
                .Include(f => f.Threads)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            return await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Forum?> GetForumByIdAsync(int forumId)
        {
            return await _context.Forums
                .Include(f => f.User)
                .Include(f => f.Threads)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.ForumId == forumId);
        }

        public async Task<Forum> CreateForumAsync(Forum forum)
        {
            forum.CreatedAt = DateTime.UtcNow;
            forum.UpdatedAt = DateTime.UtcNow;
            forum.ViewCount = 0;
            forum.LikeCount = 0;

            _context.Forums.Add(forum);
            await _context.SaveChangesAsync();
            return forum;
        }

        public async Task<Forum> UpdateForumAsync(Forum forum)
        {
            forum.UpdatedAt = DateTime.UtcNow;
            _context.Forums.Update(forum);
            await _context.SaveChangesAsync();
            return forum;
        }

        public async Task<bool> DeleteForumAsync(int forumId)
        {
            var forum = await _context.Forums.FindAsync(forumId);
            if (forum == null) return false;

            _context.Forums.Remove(forum);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Forum>> SearchForumsAsync(string keyword, int page = 1, int pageSize = 20)
        {
            return await _context.Forums
                .Include(f => f.User)
                .Include(f => f.Threads)
                .Where(f => f.Title.Contains(keyword) || f.Content.Contains(keyword))
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Forum>> GetUserForumsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Forums
                .Include(f => f.User)
                .Include(f => f.Threads)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Forum>> GetPopularForumsAsync(int count = 10)
        {
            return await _context.Forums
                .Include(f => f.User)
                .Include(f => f.Threads)
                .OrderByDescending(f => f.ViewCount)
                .ThenByDescending(f => f.LikeCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<ForumStats> GetForumStatsAsync(int forumId)
        {
            var forum = await _context.Forums
                .Include(f => f.Threads)
                .FirstOrDefaultAsync(f => f.ForumId == forumId);

            if (forum == null)
            {
                return new ForumStats();
            }

            var replyCount = forum.Threads?.Count ?? 0;
            var lastActivity = forum.UpdatedAt;
            if (forum.Threads?.Any() == true)
            {
                lastActivity = forum.Threads.Max(t => t.CreatedAt);
            }

            return new ForumStats
            {
                ViewCount = forum.ViewCount,
                ReplyCount = replyCount,
                LikeCount = forum.LikeCount,
                LastActivity = lastActivity
            };
        }
    }
}