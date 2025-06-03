using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ApplicationDbContext _context;

        public PortfolioController(IPortfolioService portfolioService, ApplicationDbContext context)
        {
            _portfolioService = portfolioService;
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

        }

        // GET: /Portfolio
        public async Task<IActionResult> Index()
        {
            Guid userId = GetCurrentUserId();
            var portfolios = await _portfolioService.GetAllPortfoliosAsync(userId);
            return View(portfolios);
        }

        // GET: /Portfolio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Portfolio/Create
        [HttpPost]
        public async Task<IActionResult> Create(Portfolio portfolio)
        {
            portfolio.UserId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                
                await _portfolioService.AddPortfolioAsync(portfolio);
                TempData["Success"] = "Portfolio created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(portfolio);
        }

        // GET: /Portfolio/Edit/id
        public async Task<IActionResult> Edit(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();
            return View(portfolio);
        }

        // POST: /Portfolio/Edit/id
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, Portfolio portfolio)
        {
            if (id != portfolio.PortfolioId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _portfolioService.UpdatePortfolioAsync(portfolio);
                TempData["Success"] = "Portfolio updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(portfolio);
        }

        // GET: /Portfolio/Details/id
        public async Task<IActionResult> Details(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();
            return View(portfolio);
        }

        // GET: /Portfolio/Delete/id
        public async Task<IActionResult> Delete(Guid id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();
            return View(portfolio);
        }

        // POST: /Portfolio/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _portfolioService.DeletePortfolioAsync(id);
            TempData["Success"] = "Portfolio deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // 🔁 POST: /Portfolio/Clone
        [HttpPost]
        public async Task<IActionResult> Clone(Guid id)
        {
            var original = await _context.Portfolios
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.PortfolioId == id);

            if (original == null) return NotFound();

            var clonedPortfolio = new Portfolio
            {
                UserId = original.UserId,
                PortfolioName = original.PortfolioName + " (Copy)",
                Description = original.Description,
                CreatedDate = DateTime.UtcNow
            };

            _context.Portfolios.Add(clonedPortfolio);
            await _context.SaveChangesAsync();

            foreach (var asset in original.Assets)
            {
                var clonedAsset = new Asset
                {
                    PortfolioId = clonedPortfolio.PortfolioId,
                    AssetName = asset.AssetName,
                    AssetType = asset.AssetType,
                    Quantity = asset.Quantity,
                    PurchasePrice = asset.PurchasePrice
                };
                _context.Assets.Add(clonedAsset);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Portfolio cloned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // 📊 GET: /Portfolio/AllocationChart/id
        public async Task<IActionResult> AllocationChart(Guid id)
        {
            var data = await _context.Assets
                .Where(a => a.PortfolioId == id)
                .GroupBy(a => a.AssetType)
                .Select(g => new
                {
                    Type = g.Key,
                    TotalAmount = g.Sum(a => a.PurchasePrice * a.Quantity)
                })
                .ToListAsync();

            ViewBag.PortfolioId = id;
            return View(data);
        }
    }
}
