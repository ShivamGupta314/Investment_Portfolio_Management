using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManagement.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IPortfolioService _portfolioService;

        public AssetController(IAssetService assetService, IPortfolioService portfolioService)
        {
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
            var asset = new Asset { PortfolioId = portfolioId };
            return View(asset);
        }

        // POST: /Asset/Create
        [HttpPost]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (ModelState.IsValid)
            {
                await _assetService.AddAssetAsync(asset);
                TempData["Success"] = "Asset added successfully.";
                return RedirectToAction(nameof(Index), new { portfolioId = asset.PortfolioId });
            }
            return View(asset);
        }

        // GET: /Asset/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();
            return View(asset);
        }

        // POST: /Asset/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, Asset asset)
        {
            if (id != asset.AssetId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _assetService.UpdateAssetAsync(asset);
                TempData["Success"] = "Asset updated successfully.";
                return RedirectToAction(nameof(Index), new { portfolioId = asset.PortfolioId });
            }
            return View(asset);
        }

        // GET: /Asset/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();
            return View(asset);
        }

        // GET: /Asset/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();
            return View(asset);
        }

        // POST: /Asset/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            Guid portfolioId = asset.PortfolioId;
            await _assetService.DeleteAssetAsync(id);
            TempData["Success"] = "Asset deleted successfully.";
            return RedirectToAction(nameof(Index), new { portfolioId });
        }
    }
}
