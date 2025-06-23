
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public AdminController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var userCount = _context.Users.Count();
            var portfolioCount = _context.Portfolios.Count();
            var averagePortfolioValue = _context.Portfolios.Any() ? _context.Portfolios.Average(p => p.TotalValue) : 0;
            var commonAssetType = _context.Assets
                .GroupBy(a => a.AssetType)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var totalassets = _context.Assets.Count();

            ViewBag.UserCount = userCount;
            ViewBag.PortfolioCount = portfolioCount;
            ViewBag.AveragePortfolioValue = averagePortfolioValue;
            ViewBag.CommonAssetType = commonAssetType ?? "N/A";
            ViewBag.UserRolesData = _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new { role = g.Key, count = g.Count() })
            .ToList();
            ViewBag.TotalAssest = totalassets;
            var thisYear = DateTime.Now.Year;

            // Build complete month list (Jan–Dec)
            var monthlyLabels = Enumerable.Range(1, 12)
                .Select(m => new DateTime(thisYear, m, 1))
                .ToList();

            // Query and group registrations by month of current year
            var registrations = _context.Users
                .Where(u => u.RegisteredDate.Year == thisYear)
                .GroupBy(u => u.RegisteredDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Month, g => g.Count);

            // Merge with months to fill zeroes where needed
            var monthlyData = monthlyLabels
                .Select(d => new {
                    month = d.ToString("MMM"),
                    count = registrations.ContainsKey(d.Month) ? registrations[d.Month] : 0
                }).ToList();

            // Scale values between 1–100 (if desired, normalize values here)
            ViewBag.MonthlyRegistrations = monthlyData;



            return View();
        }
        [HttpGet]
        public IActionResult ManageAssets()
        {
            var allAssets = _context.Assets.OrderBy(a => a.Name).ToList();

            // 2. Put the list of assets into the ViewBag.
            ViewBag.AllAssets = allAssets;
            return View();
        }

        [HttpPost]
        public IActionResult ManageAssets(Asset asset)
        {
            asset.AssetId = Guid.NewGuid();
            _context.Assets.Add(asset);
            _context.SaveChanges();
            TempData["Success"] = "Asset created successfully.";
            return RedirectToAction("ManageAssets");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        public async Task<User?> FindUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await FindUserByIdAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userService.FindUserByIdAsync(id);
                if (user == null || user.Username == "admin")
                {
                    return Json(new { success = false, message = "User not found or cannot delete admin." });
                }

                var result = await _userService.DeleteUserAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                return Json(new { success = false, message = "Error deleting user: " + ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditAsset(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            // We need to pass the list of all assets again for the table on the side.
            ViewBag.AllAssets = await _context.Assets.OrderBy(a => a.Name).ToListAsync();

            // Pass the specific asset to be edited as the main model.
            return View("ManageAssets", asset);
        }

        // --- NEW: ACTION TO PROCESS THE EDIT FORM SUBMISSION ---
        [HttpPost]
        public async Task<IActionResult> EditAsset(Asset asset)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Asset updated successfully.";
                    return RedirectToAction(nameof(ManageAssets));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Assets.Any(e => e.AssetId == asset.AssetId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If model state is invalid, return to the view with the data
            ViewBag.AllAssets = await _context.Assets.OrderBy(a => a.Name).ToListAsync();
            return View("ManageAssets", asset);
        }

        // --- NEW: ACTION TO HANDLE DELETING AN ASSET (used by AJAX) ---
        [HttpPost]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return Json(new { success = false, message = "Asset not found." });
            }

            // Optional: Check if the asset is in use by any investments
            var isInUse = await _context.Investments.AnyAsync(i => i.AssetId == id);
            if (isInUse)
            {
                return Json(new { success = false, message = "Cannot delete asset because it is currently part of one or more investments." });
            }

            try
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = "An error occurred while deleting the asset." });
            }
        }

    }
}
