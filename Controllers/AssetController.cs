using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAssetService _assetService;
        private readonly IPortfolioService _portfolioService;

        public AssetController(ApplicationDbContext context ,IAssetService assetService, IPortfolioService portfolioService)
        {
            _context = context;
            _assetService = assetService;
            _portfolioService = portfolioService;
        }

        // GET: /Asset/Index/{portfolioId}
        public async Task<IActionResult> Index(Guid portfolioId)
        {
            var assets = await _assetService.GetAssetsByPortfolioIdAsync(portfolioId);
            ViewBag.PortfolioId = portfolioId;
            return View(assets);
        }


        // GET: /Asset/Create/{portfolioId}
        public IActionResult Create(Guid portfolioId)
        {
            var assetTemplates = _context.Assets.Where(a => a.PortfolioId == null).ToList(); // Admin created assets
            ViewBag.AssetTemplates = assetTemplates;
            ViewBag.PortfolioId = portfolioId;
            return View(new Asset { PortfolioId = portfolioId });
        }

        // POST: /Asset/Create
        [HttpPost]
        public async Task<IActionResult> Create(Asset model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return View(model);
            }

            model.AssetId = Guid.NewGuid();
            model.CurrentPrice = model.BasePrice;
            model.TotalValue = model.BasePrice * model.Quantity;

            _context.Assets.Add(model);

            var portfolio = await _context.Portfolios.FindAsync(model.PortfolioId);
            if (portfolio != null)
            {
                portfolio.TotalValue += model.TotalValue;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { portfolioId = model.PortfolioId });
        }

    }
}