using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.ViewModels;
using System.Linq; // Make sure this is included for LINQ methods like Select

public class InvestmentController : Controller
{
    private readonly IInvestmentService _investmentService;
    private readonly ApplicationDbContext _context;

    public InvestmentController(ApplicationDbContext context, IInvestmentService investmentService)
    {
        _context = context;
        _investmentService = investmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var investments = await _investmentService.GetUserInvestmentsAsync(userId);
        return View(investments);
    }

    [HttpGet]
    public async Task<IActionResult> Create(/* Optionally, a portfolio id can be passed */ Guid? portfolioId)
    {
        var assets = await _context.Assets.ToListAsync();

        if (assets == null || !assets.Any())
        {
            return NotFound("No assets found.");
        }

        // Pass a new ViewModel instance, initializing PortfolioId if provided.
        var viewModel = new InvestmentViewModel
        {
            PortfolioId = portfolioId,
            AvailableAssets = assets
        };

        return View(viewModel); // Pass ViewModel directly
    }

    [HttpPost]
    public async Task<IActionResult> Create(InvestmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableAssets = await _context.Assets.ToListAsync();
            return View(model);
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // If a portfolio was provided, optionally verify its existence
        if (model.PortfolioId.HasValue)
        {
            var portfolioExists = await _context.Portfolios.AnyAsync(p => p.PortfolioId == model.PortfolioId.Value);
            if (!portfolioExists)
            {
                return NotFound("Portfolio not found.");
            }
        }

        var investment = new Investment
        {
            InvestmentId = Guid.NewGuid(),
            UserId = userId,
            AssetId = model.AssetId,
            // PortfolioId will be null if not provided
            PortfolioId = model.PortfolioId,
            Quantity = model.Quantity,
            InvestmentDate = DateTime.Now
        };

        await _investmentService.CreateInvestmentAsync(investment);

        TempData["Success"] = "Investment added successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Fetches live investment data for the current user, including asset details.
    /// This endpoint is used by AJAX calls from the frontend for real-time updates.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserInvestmentsLive()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // IMPORTANT: Ensure GetUserInvestmentsAsync eagerly loads the 'Asset' navigation property.
        // For example, if your service uses EF Core, it should include .Include(i => i.Asset)
        // to retrieve Asset.Name and Asset.CurrentPrice.
        var investments = await _investmentService.GetUserInvestmentsAsync(userId);

        var liveData = investments.Select(inv => new
        {
            investmentId = inv.InvestmentId,
            assetId = inv.Asset?.AssetId ?? Guid.Empty, // Needed for chart functionality
            assetName = inv.Asset?.Name,
            quantity = inv.Quantity,
            currentPrice = inv.Asset?.CurrentPrice ?? 0.0M, // Use 0.0M if CurrentPrice is null
            purchasePrice = inv.PurchasePrice,
            // --- FIX START ---
            // Send the raw DateTime object. The default JSON serializer in ASP.NET Core
            // will convert this to an ISO 8601 string (e.g., "2023-10-27T10:30:00").
            // JavaScript's new Date() constructor handles this format reliably.
            investmentDate = inv.InvestmentDate
            // --- FIX END ---
        }).ToList();

        return Ok(liveData); // Returns JSON with anonymous objects
    }
}