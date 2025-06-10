using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.ViewModels;

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
}
