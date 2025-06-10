
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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

            ViewBag.UserCount = userCount;
            ViewBag.PortfolioCount = portfolioCount;
            ViewBag.AveragePortfolioValue = averagePortfolioValue;
            ViewBag.CommonAssetType = commonAssetType ?? "N/A";

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
    }
}
