using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class UserSignInStatsController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public UserSignInStatsController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/UserSignInStats
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var signInStats = await _context.UserSignInStat
                .Where(s => s.UserID == userId)
                .OrderByDescending(s => s.SignTime)
                .ToListAsync();

            return View(signInStats);
        }

        // POST: MiniGame/UserSignInStats/SignIn
        [HttpPost]
        public async Task<IActionResult> SignIn()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            // Check if already signed in today
            var existingSignIn = await _context.UserSignInStats
                .FirstOrDefaultAsync(s => s.UserID == userId && s.SignTime.Date == today);

            if (existingSignIn != null)
            {
                return Json(new { success = false, message = "Already signed in today" });
            }

            // Create new sign-in record
            var signIn = new UserSignInStats
            {
                UserID = userId,
                SignTime = DateTime.UtcNow,
                PointsChanged = 20, // Default daily points
                ExpGained = 10, // Default daily experience
                CouponGained = "0", // No coupon by default
                PointsChangedTime = DateTime.UtcNow,
                ExpGainedTime = DateTime.UtcNow,
                CouponGainedTime = DateTime.UtcNow
            };

            _context.UserSignInStats.Add(signIn);

            // Update user wallet
            var wallet = await _context.User_Wallet
                .FirstOrDefaultAsync(w => w.User_Id == userId);
            if (wallet != null)
            {
                wallet.User_Point += signIn.PointsChanged;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sign in successful", points = signIn.PointsChanged });
        }

        private int GetCurrentUserId()
        {
            // TODO: Implement proper user ID retrieval from authentication
            return 1; // Placeholder
        }
    }
}