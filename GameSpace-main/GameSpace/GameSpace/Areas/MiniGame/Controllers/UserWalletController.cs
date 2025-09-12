using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class UserWalletController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public UserWalletController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/UserWallet
        public async Task<IActionResult> Index()
        {
            // Get current user's wallet information
            var userId = GetCurrentUserId();
            var wallet = await _context.User_Wallet
                .FirstOrDefaultAsync(w => w.User_Id == userId);

            if (wallet == null)
            {
                // Create wallet if it doesn't exist
                wallet = new User_Wallet
                {
                    User_Id = userId,
                    User_Point = 0
                };
                _context.User_Wallet.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return View(wallet);
        }

        // GET: MiniGame/UserWallet/History
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            var history = await _context.WalletHistory
                .Where(h => h.UserID == userId)
                .OrderByDescending(h => h.ChangeTime)
                .ToListAsync();

            return View(history);
        }

        private int GetCurrentUserId()
        {
            // TODO: Implement proper user ID retrieval from authentication
            return 1; // Placeholder
        }
    }
}